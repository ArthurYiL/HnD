﻿/*
	This file is part of HnD.
	HnD is (c) 2002-2020 Solutions Design.
	https://www.llblgen.com
	https://www.sd.nl

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
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SD.HnD.BL;
using SD.HnD.DALAdapter.EntityClasses;
using SD.HnD.DALAdapter.HelperClasses;
using SD.HnD.Gui.Models;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;

namespace SD.HnD.Gui.Controllers
{
	/// <summary>
	/// Controller for the default action 
	/// </summary>
	/// <remarks>The async methods don't use an Async suffix. This is by design, due to: https://github.com/dotnet/aspnetcore/issues/8998</remarks>
	public class HomeController : Controller
	{
		private IMemoryCache _cache;


		public HomeController(IMemoryCache cache)
		{
			ArgumentVerifier.CantBeNull(cache, nameof(cache));
			_cache = cache;
		}


		[HttpGet]
		public ActionResult Help()
		{
			return View();
		}


		[HttpGet]
		public async Task<ActionResult> Index()
		{
			var accessableForums = this.HttpContext.Session.GetForumsWithActionRight(ActionRights.AccessForum);
			var forumsWithThreadsFromOthers = this.HttpContext.Session.GetForumsWithActionRight(ActionRights.ViewNormalThreadsStartedByOthers);
			var allSections = await _cache.GetAllSectionsAsync();
			var model = new HomeData();
			model.ForumDataPerDisplayedSection = await ForumGuiHelper.GetAllAvailableForumsAggregatedData(allSections, accessableForums, forumsWithThreadsFromOthers,
																										  this.HttpContext.Session.GetUserID());

			// create a view on the sections to display and filter the view with a filter on sectionid: a sectionid must be part of the list of ids in the hashtable with per sectionid 
			// aggregate forum data. 
			model.SectionsFiltered = new EntityView2<SectionEntity>(allSections, SectionFields.SectionID.In(model.ForumDataPerDisplayedSection.Keys.ToList()));

			model.NickName = this.HttpContext.Session.GetUserNickName();
			model.UserLastVisitDate = this.HttpContext.Session.IsLastVisitDateValid() ? this.HttpContext.Session.GetLastVisitDate() : (DateTime?)null;
			model.IsAnonymousUser = this.HttpContext.Session.IsAnonymousUser();
			return View(model);
		}


		[HttpGet]
		public ActionResult TermsOfUsage()
		{
			return View();
		}
	}
}