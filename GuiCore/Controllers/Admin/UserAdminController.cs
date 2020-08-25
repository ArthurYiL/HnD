using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SD.HnD.BL;
using SD.HnD.Gui.Classes;
using SD.HnD.Gui.Models.Admin;

namespace SD.HnD.Gui.Controllers
{
	public class UserAdminController : Controller
	{
		private IMemoryCache _cache;

		public UserAdminController(IMemoryCache cache)
		{
			_cache = cache;
		}
		
		
		[HttpGet]
		[Authorize]
		public ActionResult BanUnbanUser()
		{
			if(!this.HttpContext.Session.HasSystemActionRights() || !this.HttpContext.Session.HasSystemActionRight(ActionRights.SystemManagement))
			{
				return RedirectToAction("Index", "Home");
			}

			var data = new BanUnbanUserData();
			FillUserDataForState(data.FindUserData, AdminFindUserState.Start, string.Empty, "BanUnbanUser_Find");
			return View("~/Views/Admin/BanUnbanUser.cshtml", data);
		}


		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public ActionResult BanUnbanUser_Find(FindUserData data)
		{
			if(!this.HttpContext.Session.HasSystemActionRights() || !this.HttpContext.Session.HasSystemActionRight(ActionRights.SystemManagement))
			{
				return RedirectToAction("Index", "Home");
			}

			BanUnbanUserData newData = null;
			if(data.IsAnythingChecked)
			{
				FillUserDataForState(data, AdminFindUserState.UsersFound, "Ban / Unban selected user", "BanUnbanUser_UserSelected");
				newData = new BanUnbanUserData(data);
			}
			else
			{
				newData = new BanUnbanUserData();
			}
			return View("~/Views/Admin/BanUnbanUser.cshtml", newData);
		}


		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public ActionResult BanUnbanUser_UserSelected(FindUserData data, string submitAction)
		{
			if(!this.HttpContext.Session.HasSystemActionRights() || !this.HttpContext.Session.HasSystemActionRight(ActionRights.SystemManagement))
			{
				return RedirectToAction("Index", "Home");
			}

			if(submitAction == "SearchAgain")
			{
				return BanUnbanUser();
			}

			if(submitAction != "PerformAction")
			{
				return RedirectToAction("Index", "Home");
			}

			if(data.SelectedUserIDs==null || data.SelectedUserIDs.Count<=0)
			{
				return BanUnbanUser_Find(data);
			}

			FillUserDataForState(data, AdminFindUserState.FinalAction, string.Empty, "BanUnbanUser_ToggleBanFlag");
			return View("~/Views/Admin/BanUnbanUser.cshtml", new BanUnbanUserData(data));
		}


		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public ActionResult BanUnbanUser_ToggleBanFlag(FindUserData data, string submitAction)
		{
			if(!this.HttpContext.Session.HasSystemActionRights() || !this.HttpContext.Session.HasSystemActionRight(ActionRights.SystemManagement))
			{
				return RedirectToAction("Index", "Home");
			}

			if(submitAction != "ToggleBanFlag")
			{
				return RedirectToAction("Index", "Home");
			}

			if(data.SelectedUserIDs==null || data.SelectedUserIDs.Count<=0)
			{
				return BanUnbanUser_Find(data);
			}

			int userIdToToggleBanFlagOf = data.SelectedUserIDs.FirstOrDefault();
			bool result = UserManager.ToggleBanFlagValue(userIdToToggleBanFlagOf, out bool newBanFlagValue);
			if(newBanFlagValue)
			{
				var user = UserGuiHelper.GetUser(userIdToToggleBanFlagOf);
				ApplicationAdapter.AddUserToListToBeLoggedOutByForce(user.NickName);
			}
			FillUserDataForState(data, AdminFindUserState.PostAction, string.Empty, string.Empty);
			var viewData = new BanUnbanUserData(data);
			if(result)
			{
				viewData.FinalActionResult = newBanFlagValue ? "The user now banned" : "The user has been unbanned";
			}
			else
			{
				viewData.FinalActionResult = "Toggling the ban flag failed.";
			}

			return View("~/Views/Admin/BanUnbanUser.cshtml", viewData);
		}


		private static void FillUserDataForState(FindUserData data, AdminFindUserState stateToFillDataFor, string actionButtonText, string actionToPostTo)
		{
			data.Roles = SecurityGuiHelper.GetAllRoles();
			switch(stateToFillDataFor)
			{
				case AdminFindUserState.Start:
					// no-op
					break;
				case AdminFindUserState.UsersFound:
					data.FoundUsers = UserGuiHelper.FindUsers(data.FilterOnRole, data.SelectedRoleID, data.FilterOnNickName,
															  data.SpecifiedNickName, data.FilterOnEmailAddress,
															  data.SpecifiedEmailAddress);
					break;
				case AdminFindUserState.FinalAction:
				case AdminFindUserState.PostAction:
					data.SelectedUsers = UserGuiHelper.GetAllUsersInRange(data.SelectedUserIDs);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(stateToFillDataFor), stateToFillDataFor, null);
			}
			data.FindUserState = stateToFillDataFor;
			data.ActionButtonText = actionButtonText;
			data.ActionToPostTo = actionToPostTo;
		}
	}
}