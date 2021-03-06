/*
	This file is part of HnD.
	HnD is (c) 2002-2020 Solutions Design.
    http://www.llblgen.com
	http://www.sd.nl

	HnD is free software; you can redistribute it and/or modify
	it under the terms of version 2 of the GNU General Public License as published by
	the Free Software Foundation.

	HnD is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with HnD, please see the LICENSE.txt file; if not, write to the Free Software
	Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections.Generic;
using System.Text;
using SD.HnD.DALAdapter.EntityClasses;
using SD.HnD.DALAdapter.HelperClasses;
using System.Data;
using System.Threading.Tasks;
using SD.HnD.DALAdapter.DatabaseSpecific;
using SD.HnD.DALAdapter.FactoryClasses;
using SD.HnD.DTOs.DtoClasses;
using SD.HnD.DTOs.Persistence;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;

namespace SD.HnD.BL
{
	/// <summary>
	/// General class for support queue management related tasks
	/// </summary>
	public static class SupportQueueManager
	{
		/// <summary>
		/// Creates a new support queue.
		/// </summary>
		/// <param name="newDataDto">the dto with the data for the new queue</param>
		/// <returns>the id of the new queue or 0 if failed</returns>
		public static async Task<int> CreateNewSupportQueueAsync(SupportQueueDto newDataDto)
		{
			var newSupportQueue = new SupportQueueEntity();
			newSupportQueue.UpdateFromSupportQueue(newDataDto);
			using(var adapter = new DataAccessAdapter())
			{
				var result = await adapter.SaveEntityAsync(newSupportQueue).ConfigureAwait(false);
				return result ? newSupportQueue.QueueID : 0;
			}
		}


		/// <summary>
		/// Modifies the support queue definition data.
		/// </summary>
		/// <param name="toUpdate">the dto containing the data of the support queue to update</param>
		/// <returns>true if succeeded, false otherwise</returns>
		public static async Task<bool> ModifySupportQueueAsync(SupportQueueDto toUpdate)
		{
			if(toUpdate == null)
			{
				return false;
			}

			var toModify = await SupportQueueGuiHelper.GetSupportQueueAsync(toUpdate.QueueID);
			if(toModify == null)
			{
				// not found
				return false;
			}

			toModify.UpdateFromSupportQueue(toUpdate);
			using(var adapter = new DataAccessAdapter())
			{
				return await adapter.SaveEntityAsync(toModify).ConfigureAwait(false);
			}
		}


		/// <summary>
		/// Deletes the support queue with the ID specified.
		/// </summary>
		/// <param name="queueId">The queue ID of the queue to delete.</param>
		/// <returns>true if succeeded, false otherwise</returns>
		/// <remarks>All threads in the queue are automatically de-queued and not in a queue anymore. The Default support queue
		/// for forums which have this queue as the default support queue is reset to null.</remarks>
		public static async Task<bool> DeleteSupportQueueAsync(int queueId)
		{
			using(var adapter = new DataAccessAdapter())
			{
				// we'll do several actions in one atomic transaction, so start a transaction first. 
				await adapter.StartTransactionAsync(IsolationLevel.ReadCommitted, "DeleteSupportQ").ConfigureAwait(false);
				try
				{
					// first reset all the FKs in Forum to NULL if they point to this queue.
					await adapter.UpdateEntitiesDirectlyAsync(new ForumEntity() {DefaultSupportQueueID = null},
															  new RelationPredicateBucket(ForumFields.DefaultSupportQueueID.Equal(queueId))).ConfigureAwait(false);
					;

					// delete all SupportQueueThread entities which refer to this queue. This will make all threads which are in this queue become queue-less.
					await adapter.DeleteEntitiesDirectlyAsync(typeof(SupportQueueThreadEntity),
															  new RelationPredicateBucket(SupportQueueThreadFields.QueueID.Equal(queueId))).ConfigureAwait(false);
					;

					// it's now time to delete the actual supportqueue entity.
					var result = await adapter.DeleteEntitiesDirectlyAsync(typeof(SupportQueueEntity),
																		   new RelationPredicateBucket(SupportQueueFields.QueueID.Equal(queueId))).ConfigureAwait(false);
					;

					// done so commit the transaction.
					adapter.Commit();
					return (result > 0);
				}
				catch
				{
					adapter.Rollback();
					throw;
				}
			}
		}


		/// <summary>
		/// Claims the thread specified for the user specified. As the thread can be in one queue at a time, it simply has to update the SupportQueueThread entity.
		/// </summary>
		/// <param name="userId">The user ID.</param>
		/// <param name="threadId">The thread ID.</param>
		public static Task ClaimThreadAsync(int userId, int threadId)
		{
			return UpdateClaimOnThreadAsync(userId, threadId, claim: true);
		}


		/// <summary>
		/// Releases the claim on the thread specified. As the thread can be in one queue at a time, it simply has to update the SupportQueueThread entity.
		/// </summary>
		/// <param name="threadId">The thread ID.</param>
		public static Task ReleaseClaimOnThreadAsync(int threadId)
		{
			return UpdateClaimOnThreadAsync(-1, threadId, claim: false);
		}


		/// <summary>
		/// Removes the thread with the threadid specified from the queue it's in. A thread can be in a single queue, so we don't need the queueID.
		/// </summary>
		/// <param name="threadId">The thread ID.</param>
		/// <param name="adapter">The adapter with a live transaction. Can be null, in which case a local adapter is used.</param>
		public static async Task RemoveThreadFromQueueAsync(int threadId, IDataAccessAdapter adapter)
		{
			var localAdapter = adapter == null;
			var adapterToUse = adapter ?? new DataAccessAdapter();
			try
			{
				// delete the SupportQueueThread entity for this thread directly from the db
				await adapterToUse.DeleteEntitiesDirectlyAsync(typeof(SupportQueueThreadEntity),
															   new RelationPredicateBucket(SupportQueueThreadFields.ThreadID.Equal(threadId)))
								  .ConfigureAwait(false);

				// don't commit the current transaction if specified, simply return to caller.
			}
			finally
			{
				if(localAdapter)
				{
					adapterToUse.Dispose();
				}
			}
		}


		/// <summary>
		/// Adds the thread with the ID specified to the support queue with the ID specified
		/// </summary>
		/// <param name="threadId">The thread ID.</param>
		/// <param name="queueId">The queue ID.</param>
		/// <param name="userId">The user ID of the user causing this thread to be placed in the queue specified.</param>
		/// <param name="adapter">The live adapter with an active transaction. Can be null, in which case a local transaction is used.</param>
		/// <remarks>first removes the thread from a queue if it's in a queue</remarks>
		public static async Task AddThreadToQueueAsync(int threadId, int queueId, int userId, IDataAccessAdapter adapter)
		{
			var localAdapter = adapter == null;
			var adapterToUse = adapter ?? new DataAccessAdapter();
			try
			{
				if(localAdapter)
				{
					await adapterToUse.StartTransactionAsync(IsolationLevel.ReadCommitted, "AddThreadToQueue").ConfigureAwait(false);
				}

				// first remove the thread from any queue it's in. 
				await RemoveThreadFromQueueAsync(threadId, adapterToUse);

				// then add it to the queue specified.
				var supportQueueThread = new SupportQueueThreadEntity
										 {
											 ThreadID = threadId,
											 QueueID = queueId,
											 PlacedInQueueByUserID = userId,
											 PlacedInQueueOn = DateTime.Now
										 };
				await adapterToUse.SaveEntityAsync(supportQueueThread).ConfigureAwait(false);
				if(localAdapter)
				{
					adapterToUse.Commit();
				}
			}
			catch
			{
				if(localAdapter)
				{
					adapterToUse.Rollback();
				}

				throw;
			}
			finally
			{
				if(localAdapter)
				{
					adapterToUse.Dispose();
				}
			}
		}


		/// <summary>
		/// Updates the claim data on thread. 
		/// </summary>
		/// <param name="userId">The user identifier.</param>
		/// <param name="threadId">The thread identifier.</param>
		/// <param name="claim">if set to <c>true</c> a claim by userID is set on the thread, otherwise an existing claim is released.</param>
		private static async Task UpdateClaimOnThreadAsync(int userId, int threadId, bool claim)
		{
			using(var adapter = new DataAccessAdapter())
			{
				var q = new QueryFactory().SupportQueueThread.Where(SupportQueueThreadFields.ThreadID.Equal(threadId));
				var supportQueueThread = adapter.FetchFirst(q);
				if(supportQueueThread == null)
				{
					// not found
					return;
				}

				// simply overwrite an existing claim if any.
				if(claim)
				{
					supportQueueThread.ClaimedByUserID = userId;
					supportQueueThread.ClaimedOn = DateTime.Now;
				}
				else
				{
					supportQueueThread.ClaimedByUserID = null;
					supportQueueThread.ClaimedOn = null;
				}

				// done, save it
				await adapter.SaveEntityAsync(supportQueueThread).ConfigureAwait(false);
			}
		}
	}
}