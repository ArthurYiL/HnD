﻿//------------------------------------------------------------------------------
// <auto-generated>This code was generated by LLBLGen Pro v5.7.</auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SD.LLBLGen.Pro.QuerySpec;
using SD.HnD.DALAdapter.HelperClasses;

namespace SD.HnD.DTOs.Persistence
{
	/// <summary>Static class for (extension) methods for fetching and projecting instances of SD.HnD.DTOs.DtoClasses.MessageInThreadDto from / to the entity model.</summary>
	public static partial class MessageInThreadPersistence
	{
		private static readonly System.Linq.Expressions.Expression<Func<SD.HnD.DALAdapter.EntityClasses.MessageEntity, SD.HnD.DTOs.DtoClasses.MessageInThreadDto>> _projectorExpression = CreateProjectionFunc();
		private static readonly Func<SD.HnD.DALAdapter.EntityClasses.MessageEntity, SD.HnD.DTOs.DtoClasses.MessageInThreadDto> _compiledProjector = CreateProjectionFunc().Compile();
	
		/// <summary>Empty static ctor for triggering initialization of static members in a thread-safe manner</summary>
		static MessageInThreadPersistence() { }
	
		/// <summary>Extension method which produces a projection to SD.HnD.DTOs.DtoClasses.MessageInThreadDto which instances are projected from the 
		/// results of the specified baseQuery, which returns SD.HnD.DALAdapter.EntityClasses.MessageEntity instances, the root entity of the derived element returned by this query.</summary>
		/// <param name="baseQuery">The base query to project the derived element instances from.</param>
		/// <returns>IQueryable to retrieve SD.HnD.DTOs.DtoClasses.MessageInThreadDto instances</returns>
		public static IQueryable<SD.HnD.DTOs.DtoClasses.MessageInThreadDto> ProjectToMessageInThreadDto(this IQueryable<SD.HnD.DALAdapter.EntityClasses.MessageEntity> baseQuery)
		{
			return baseQuery.Select(_projectorExpression);
		}

		/// <summary>Extension method which produces a projection to SD.HnD.DTOs.DtoClasses.MessageInThreadDto which instances are projected from the 
		/// results of the specified baseQuery using QuerySpec, which returns SD.HnD.DALAdapter.EntityClasses.MessageEntity instances, the root entity of the derived element returned by this query.</summary>
		/// <param name="baseQuery">The base query to project the derived element instances from.</param>
		/// <param name="qf">The query factory used to create baseQuery.</param>
		/// <returns>DynamicQuery to retrieve SD.HnD.DTOs.DtoClasses.MessageInThreadDto instances</returns>
		public static DynamicQuery<SD.HnD.DTOs.DtoClasses.MessageInThreadDto> ProjectToMessageInThreadDto(this EntityQuery<SD.HnD.DALAdapter.EntityClasses.MessageEntity> baseQuery, SD.HnD.DALAdapter.FactoryClasses.QueryFactory qf)
		{
			return qf.Create()
				.From(baseQuery.Select(Projection.Full).As("__BQ")
					.InnerJoin(qf.User.As("__L0_0")).On(MessageFields.PostedByUserID.Source("__BQ").Equal(UserFields.UserID.Source("__L0_0")))
					.InnerJoin(qf.UserTitle.As("__L0_1")).On(UserFields.UserTitleID.Source("__L0_0").Equal(UserTitleFields.UserTitleID.Source("__L0_1"))))
				.Select(() => new SD.HnD.DTOs.DtoClasses.MessageInThreadDto()
				{
					Attachments = (List<SD.HnD.DTOs.DtoClasses.MessageInThreadDtoTypes.AttachmentDto>)qf.Attachment.TargetAs("__L1_0")
						.CorrelatedOver(MessageFields.MessageID.Source("__BQ").Equal(AttachmentFields.MessageID.Source("__L1_0")))
						.Select(() => new SD.HnD.DTOs.DtoClasses.MessageInThreadDtoTypes.AttachmentDto()
						{
							AddedOn = AttachmentFields.AddedOn.Source("__L1_0").ToValue<System.DateTime>(),
							Approved = AttachmentFields.Approved.Source("__L1_0").ToValue<System.Boolean>(),
							AttachmentID = AttachmentFields.AttachmentID.Source("__L1_0").ToValue<System.Int32>(),
							Filename = AttachmentFields.Filename.Source("__L1_0").ToValue<System.String>(),
							Filesize = AttachmentFields.Filesize.Source("__L1_0").ToValue<System.Int32>(),
						}).ToResultset(),
					MessageID = MessageFields.MessageID.Source("__BQ").ToValue<System.Int32>(),
					MessageTextAsHTML = MessageFields.MessageTextAsHTML.Source("__BQ").ToValue<System.String>(),
					PostedByUser = new SD.HnD.DTOs.DtoClasses.MessageInThreadDtoTypes.PostedByUserDto()
						{
							AmountOfPostings = UserFields.AmountOfPostings.Source("__L0_0").ToValue<Nullable<System.Int32>>(),
							IconURL = UserFields.IconURL.Source("__L0_0").ToValue<System.String>(),
							JoinDate = UserFields.JoinDate.Source("__L0_0").ToValue<Nullable<System.DateTime>>(),
							Location = UserFields.Location.Source("__L0_0").ToValue<System.String>(),
							NickName = UserFields.NickName.Source("__L0_0").ToValue<System.String>(),
							Signature = UserFields.Signature.Source("__L0_0").ToValue<System.String>(),
							UserID = UserFields.UserID.Source("__L0_0").ToValue<System.Int32>(),
							UserTitleDescription = UserTitleFields.UserTitleDescription.Source("__L0_1").ToValue<System.String>(),
						},
					PostedFromIP = MessageFields.PostedFromIP.Source("__BQ").ToValue<System.String>(),
					PostingDate = MessageFields.PostingDate.Source("__BQ").ToValue<System.DateTime>(),
					ThreadID = MessageFields.ThreadID.Source("__BQ").ToValue<System.Int32>(),
	// __LLBLGENPRO_USER_CODE_REGION_START ProjectionRegionQS_MessageInThread 
	// __LLBLGENPRO_USER_CODE_REGION_END 
				});
		}

		/// <summary>Extension method which produces a projection to SD.HnD.DTOs.DtoClasses.MessageInThreadDto which instances are projected from the
		/// SD.HnD.DALAdapter.EntityClasses.MessageEntity entity instance specified, the root entity of the derived element returned by this method.</summary>
		/// <param name="entity">The entity to project from.</param>
		/// <returns>SD.HnD.DALAdapter.EntityClasses.MessageEntity instance created from the specified entity instance</returns>
		public static SD.HnD.DTOs.DtoClasses.MessageInThreadDto ProjectToMessageInThreadDto(this SD.HnD.DALAdapter.EntityClasses.MessageEntity entity)
		{
			return _compiledProjector(entity);
		}
		
		private static System.Linq.Expressions.Expression<Func<SD.HnD.DALAdapter.EntityClasses.MessageEntity, SD.HnD.DTOs.DtoClasses.MessageInThreadDto>> CreateProjectionFunc()
		{
			return p__0 => new SD.HnD.DTOs.DtoClasses.MessageInThreadDto()
			{
				Attachments = p__0.Attachments.Select(p__1 => new SD.HnD.DTOs.DtoClasses.MessageInThreadDtoTypes.AttachmentDto()
				{
					AddedOn = p__1.AddedOn,
					Approved = p__1.Approved,
					AttachmentID = p__1.AttachmentID,
					Filename = p__1.Filename,
					Filesize = p__1.Filesize,
				}).ToList(),
				MessageID = p__0.MessageID,
				MessageTextAsHTML = p__0.MessageTextAsHTML,
				PostedByUser = new SD.HnD.DTOs.DtoClasses.MessageInThreadDtoTypes.PostedByUserDto()
				{
					AmountOfPostings = p__0.PostedByUser.AmountOfPostings,
					IconURL = p__0.PostedByUser.IconURL,
					JoinDate = p__0.PostedByUser.JoinDate,
					Location = p__0.PostedByUser.Location,
					NickName = p__0.PostedByUser.NickName,
					Signature = p__0.PostedByUser.Signature,
					UserID = p__0.PostedByUser.UserID,
					UserTitleDescription = p__0.PostedByUser.UserTitle.UserTitleDescription,
				},
				PostedFromIP = p__0.PostedFromIP,
				PostingDate = p__0.PostingDate,
				ThreadID = p__0.ThreadID,
	// __LLBLGENPRO_USER_CODE_REGION_START ProjectionRegion_MessageInThread 
	// __LLBLGENPRO_USER_CODE_REGION_END 
			};
		}
		/// <summary>Creates a primary key predicate to be used in a Where() clause in a Linq query which is executed on the database to fetch the original entity instance the specified <see cref="dto"/> object was projected from.</summary>
		/// <param name="dto">The dto object for which the primary key predicate has to be created for.</param>
		/// <returns>ready to use expression</returns>
		public static System.Linq.Expressions.Expression<Func<SD.HnD.DALAdapter.EntityClasses.MessageEntity, bool>> CreatePkPredicate(SD.HnD.DTOs.DtoClasses.MessageInThreadDto dto)
		{
			return p__0 => p__0.MessageID == dto.MessageID;
		}

		/// <summary>Creates a primary key predicate to be used in a Where() clause in a Linq query which is executed on the database to fetch the original entity instances the specified set of <see cref="dtos"/> objects was projected from.</summary>
		/// <param name="dtos">The dto objects for which the primary key predicate has to be created for.</param>
		/// <returns>ready to use expression</returns>
		public static System.Linq.Expressions.Expression<Func<SD.HnD.DALAdapter.EntityClasses.MessageEntity, bool>> CreatePkPredicate(IEnumerable<SD.HnD.DTOs.DtoClasses.MessageInThreadDto> dtos)
		{
			return p__0 => dtos.Select(p__1=>p__1.MessageID).ToList().Contains(p__0.MessageID);
		}

		/// <summary>Creates a primary key predicate to be used in a Where() clause in a Linq query on an IEnumerable in-memory set of entity instances to retrieve the original entity instance the specified <see cref="dto"/> object was projected from.</summary>
		/// <param name="dto">The dto object for which the primary key predicate has to be created for.</param>
		/// <returns>ready to use func</returns>
		public static Func<SD.HnD.DALAdapter.EntityClasses.MessageEntity, bool> CreateInMemoryPkPredicate(SD.HnD.DTOs.DtoClasses.MessageInThreadDto dto)
		{
			return p__0 => p__0.MessageID == dto.MessageID;
		}
		
		/// <summary>Updates the specified SD.HnD.DALAdapter.EntityClasses.MessageEntity entity with the values stored in the dto object specified</summary>
		/// <param name="toUpdate">the entity instance to update.</param>
		/// <param name="dto">The dto object containing the source values.</param>
		/// <remarks>The PK field of toUpdate is set only if it's not marked as readonly.</remarks>
		public static void UpdateFromMessageInThread(this SD.HnD.DALAdapter.EntityClasses.MessageEntity toUpdate, SD.HnD.DTOs.DtoClasses.MessageInThreadDto dto)
		{
			if((toUpdate == null) || (dto == null))
			{
				return;
			}
			toUpdate.MessageTextAsHTML = dto.MessageTextAsHTML;
			toUpdate.PostedFromIP = dto.PostedFromIP;
			toUpdate.PostingDate = dto.PostingDate;
			toUpdate.ThreadID = dto.ThreadID;
		}
	}
}

 