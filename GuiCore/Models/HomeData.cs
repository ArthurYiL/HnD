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
using SD.HnD.BL.TypedDataClasses;
using SD.HnD.DALAdapter.EntityClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;

namespace SD.HnD.Gui.Models
{
	/// <summary>
	/// Data for the Home / Index page. 
	/// </summary>
	public class HomeData
	{
		public string NickName { get; set; }
		public EntityView2<SectionEntity> SectionsFiltered { get; set; }
		public MultiValueHashtable<int, AggregatedForumRow> ForumDataPerDisplayedSection { get; set; }
		public DateTime? UserLastVisitDate { get; set; }
		public bool IsAnonymousUser { get; set; }
	}
}