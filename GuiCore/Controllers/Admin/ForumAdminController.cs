using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SD.HnD.BL;
using SD.HnD.DALAdapter.TypedListClasses;
using SD.HnD.Gui.Models.Admin;
using SD.HnD.Utility;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace SD.HnD.Gui.Controllers
{
	public class ForumAdminController : Controller
	{
		private IMemoryCache _cache;

		public ForumAdminController(IMemoryCache cache)
		{
			_cache = cache;
		}

		
		[HttpGet]
		[Authorize]
		public ActionResult Forums()
		{
			if(!this.HttpContext.Session.HasSystemActionRights() || !this.HttpContext.Session.HasSystemActionRight(ActionRights.SystemManagement))
			{
				return RedirectToAction("Index", "Home");
			}
			return View("~/Views/Admin/Forums.cshtml");
		}
		

		[HttpGet]
		[Authorize]
		public ActionResult<IEnumerable<ForumsWithSectionNameRow>> GetForums()
		{
			if(!this.HttpContext.Session.HasSystemActionRights() || !this.HttpContext.Session.HasSystemActionRight(ActionRights.SystemManagement))
			{
				return RedirectToAction("Index", "Home");
			}

			var forums = ForumGuiHelper.GetAllForumsWithSectionNames();
			return Ok(forums);
		}


		[HttpGet]
		[Authorize]
		public ActionResult AddForum()
		{
			if(!this.HttpContext.Session.HasSystemActionRights() || !this.HttpContext.Session.HasSystemActionRight(ActionRights.SystemManagement))
			{
				return RedirectToAction("Index", "Home");
			}

			var data = new AddEditForumData();
			FillDataSetsInModelObject(data);
			return View("~/Views/Admin/AddForum.cshtml", data);
		}


		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> AddForum(AddEditForumData data, string submitAction)
		{
			if(!this.HttpContext.Session.HasSystemActionRights() || !this.HttpContext.Session.HasSystemActionRight(ActionRights.SystemManagement))
			{
				return RedirectToAction("Index", "Home");
			}
			if(submitAction == "cancel")
			{
				return RedirectToRoute("ManageForums");
			}
			if(!ModelState.IsValid)
			{
				FillDataSetsInModelObject(data);
				return View("~/Views/Admin/AddForum.cshtml", data);
			}
			data.Sanitize();

			var welcomeMessageAsHtml = HnDGeneralUtils.TransformMarkdownToHtml(data.ForumEdited.NewThreadWelcomeText, ApplicationAdapter.GetEmojiFilenamesPerName(), 
																			   ApplicationAdapter.GetSmileyMappings());
			try
			{
				await ForumManager.CreateNewForumAsync(data.ForumEdited.SectionID, data.ForumEdited.ForumName, data.ForumEdited.ForumDescription, 
													   data.ForumEdited.HasRSSFeed, data.ForumEdited.DefaultSupportQueueID, 1, data.ForumEdited.OrderNo, 
													   data.ForumEdited.MaxAttachmentSize, data.ForumEdited.MaxNoOfAttachmentsPerMessage,
													   data.ForumEdited.NewThreadWelcomeText, welcomeMessageAsHtml);
			}
			catch(ORMQueryExecutionException ex)
			{
				ModelState.AddModelError("ForumName", "Save failed, likely due to the forum name not being unique. Please specify a unique forum name." + ex.Message);
				FillDataSetsInModelObject(data);
				return View("~/Views/Admin/AddForum.cshtml", data);
			}
			return View("~/Views/Admin/Forums.cshtml", data);
		}
		
		
		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteForum(int id)
		{
			if(!this.HttpContext.Session.HasSystemActionRights() || !this.HttpContext.Session.HasSystemActionRight(ActionRights.SystemManagement))
			{
				return RedirectToAction("Index", "Home");
			}

			var result = false;
			if(id>0)
			{
				result = ForumManager.DeleteForum(id);
			}
			if(result)
			{
				return Json(new {success = true});
			}

			return ValidationProblem("The forum wasn't deleted, due to an error. Please try again.");
		}
		

		[HttpGet]
		[Authorize]
		public ActionResult EditForum(int id)
		{
			if(!this.HttpContext.Session.HasSystemActionRights() || !this.HttpContext.Session.HasSystemActionRight(ActionRights.SystemManagement))
			{
				return RedirectToAction("Index", "Home");
			}

			var toEdit = ForumGuiHelper.GetForum(id);
			if(toEdit == null)
			{
				return RedirectToAction("Index", "Home");
			}
			var data = new AddEditForumData(toEdit);
			FillDataSetsInModelObject(data);
			return View("~/Views/Admin/EditForum.cshtml", data);
		}
		

		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> EditForum(AddEditForumData data, string submitAction, int id=0)
		{
			if(!this.HttpContext.Session.HasSystemActionRights() || !this.HttpContext.Session.HasSystemActionRight(ActionRights.SystemManagement))
			{
				return RedirectToAction("Index", "Home");
			}
			if(submitAction == "cancel")
			{
				return RedirectToRoute("ManageForums");
			}
			if(!ModelState.IsValid)
			{
				FillDataSetsInModelObject(data);
				return View("~/Views/Admin/EditForum.cshtml", data);
			}
			data.Sanitize();

			var welcomeMessageAsHtml = HnDGeneralUtils.TransformMarkdownToHtml(data.ForumEdited.NewThreadWelcomeText, ApplicationAdapter.GetEmojiFilenamesPerName(), 
																			   ApplicationAdapter.GetSmileyMappings());
			try
			{
				await ForumManager.ModifyForum(id, data.ForumEdited.SectionID, data.ForumEdited.ForumName, data.ForumEdited.ForumDescription, 
											   data.ForumEdited.HasRSSFeed, data.ForumEdited.DefaultSupportQueueID, 1, data.ForumEdited.OrderNo, 
											   data.ForumEdited.MaxAttachmentSize, data.ForumEdited.MaxNoOfAttachmentsPerMessage,
											   data.ForumEdited.NewThreadWelcomeText, welcomeMessageAsHtml);
			}
			catch(ORMQueryExecutionException ex)
			{
				ModelState.AddModelError("ForumName", "Save failed, likely due to the forum name not being unique. Please specify a unique forum name." + ex.Message);
				FillDataSetsInModelObject(data);
				return View("~/Views/Admin/AddForum.cshtml", data);
			}
			return View("~/Views/Admin/Forums.cshtml", data);
		}

		private static void FillDataSetsInModelObject(AddEditForumData data)
		{
			data.AllExistingSupportQueues = SupportQueueGuiHelper.GetAllSupportQueueDTOs();
			data.AllExistingSections = SectionGuiHelper.GetAllSectionDtos();
		}
	}
}