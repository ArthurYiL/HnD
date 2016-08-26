﻿// 
//   MarkdownDeep - http://www.toptensoftware.com/markdowndeep
//	 Copyright (C) 2010-2011 Topten Software
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this product except in 
//   compliance with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License is 
//   distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkdownDeep
{

	public class ImageInfo
	{
		public string url;
		public bool titled_image;
		public int width;
		public int height;
	}


	public class Markdown
	{
		#region Members
		private StringBuilder m_StringBuilder;
		private StringBuilder m_StringBuilderFinal;
		private StringScanner m_StringScanner;
		private SpanFormatter m_SpanFormatter;
		private Dictionary<string, LinkDefinition> m_LinkDefinitions;
		private Dictionary<string, Block> m_Footnotes;
		private List<Block> m_UsedFootnotes;
		private Dictionary<string, bool> m_UsedHeaderIDs;
		private Dictionary<string, Abbreviation> m_AbbreviationMap;
		private List<Abbreviation> m_AbbreviationList;
		private Dictionary<int, int> _sdTokenStartEndIndices;
		private int _tabIdCounter;
		#endregion

		// Constructor
		public Markdown()
		{
			HtmlClassFootnotes = "footnotes";
			m_StringBuilder = new StringBuilder();
			m_StringBuilderFinal = new StringBuilder();
			m_StringScanner = new StringScanner();
			m_SpanFormatter = new SpanFormatter(this);
			m_LinkDefinitions = new Dictionary<string, LinkDefinition>(StringComparer.CurrentCultureIgnoreCase);
			m_Footnotes = new Dictionary<string, Block>();
			m_UsedFootnotes = new List<Block>();
			m_UsedHeaderIDs = new Dictionary<string, bool>();
			this.CreatedH2IdCollector = new List<Tuple<string, string>>();
			_tabIdCounter = 0;
		}

		internal List<Block> ProcessBlocks(string str)
		{
			// Reset the list of link definitions
			m_LinkDefinitions.Clear();
			m_Footnotes.Clear();
			m_UsedFootnotes.Clear();
			m_UsedHeaderIDs.Clear();
			m_AbbreviationMap = null;
			m_AbbreviationList = null;
			_sdTokenStartEndIndices = this.DocNetMode || this.HnDMode ? new SDTokenPairFinder(str).FindPairs() : new Dictionary<int, int>();

			// Process blocks
			return new BlockProcessor(this, MarkdownInHtml).Process(str);
		}

		public string Transform(string str)
		{
			Dictionary<string, LinkDefinition> defs;
			return Transform(str, out defs);
		}

		// Transform a string
		public string Transform(string str, out Dictionary<string, LinkDefinition> definitions)
		{
			// Build blocks
			var blocks = ProcessBlocks(str);

			// Sort abbreviations by length, longest to shortest
			if (m_AbbreviationMap != null)
			{
				m_AbbreviationList = new List<Abbreviation>();
				m_AbbreviationList.AddRange(m_AbbreviationMap.Values);
				m_AbbreviationList.Sort(
					delegate(Abbreviation a, Abbreviation b)
					{
						return b.Abbr.Length - a.Abbr.Length;
					}
				);
			}

			// Setup string builder
			StringBuilder sb = m_StringBuilderFinal;
			sb.Length = 0;

			if (SummaryLength != 0)
			{
				// Render all blocks
				for (int i = 0; i < blocks.Count; i++)
				{
					var b = blocks[i];
					b.RenderPlain(this, sb);

					if (SummaryLength>0 && sb.Length > SummaryLength)
						break;
				}

			}
			else
			{
				int iSection = -1;

				// Leading section (ie: plain text before first heading)
				if (blocks.Count > 0 && !IsSectionHeader(blocks[0]))
				{
					iSection = 0;
					OnSectionHeader(sb, 0);
					OnSectionHeadingSuffix(sb, 0);
				}

				// Render all blocks
				for (int i = 0; i < blocks.Count; i++)
				{
					var b = blocks[i];

					// New section?
					if (IsSectionHeader(b))
					{
						// Finish the previous section
						if (iSection >= 0)
						{
							OnSectionFooter(sb, iSection);
						}

						// Work out next section index
						iSection = iSection < 0 ? 1 : iSection + 1;

						// Section header
						OnSectionHeader(sb, iSection);

						// Section Heading
						b.Render(this, sb);

						// Section Heading suffix
						OnSectionHeadingSuffix(sb, iSection);
					}
					else
					{
						// Regular section
						b.Render(this, sb);
					}
				}

				// Finish final section
				if (blocks.Count > 0)
					OnSectionFooter(sb, iSection);

				// Render footnotes
				if (m_UsedFootnotes.Count > 0)
				{
					sb.Append("\n<div class=\"");
					sb.Append(HtmlClassFootnotes);
					sb.Append("\">\n");
					sb.Append("<hr />\n");
					sb.Append("<ol>\n");
					for (int i = 0; i < m_UsedFootnotes.Count; i++)
					{
						var fn = m_UsedFootnotes[i];

						sb.Append("<li id=\"fn:");
						sb.Append((string)fn.Data);	// footnote id
						sb.Append("\">\n");


						// We need to get the return link appended to the last paragraph
						// in the footnote
						string strReturnLink = string.Format("<a href=\"#fnref:{0}\" rev=\"footnote\">&#8617;</a>", (string)fn.Data);

						// Get the last child of the footnote
						if(fn.Children.Count > 0)
						{
							var child = fn.Children[fn.Children.Count - 1];
							if(child.BlockType == BlockType.p)
							{
								child.BlockType = BlockType.p_footnote;
								child.Data = strReturnLink;
							}
							else
							{
								child = CreateBlock();
								child.ContentLen = 0;
								child.BlockType = BlockType.p_footnote;
								child.Data = strReturnLink;
								fn.Children.Add(child);
							}
						}
						fn.Render(this, sb);

						sb.Append("</li>\n");
					}
					sb.Append("</ol>\n");
					sb.Append("</div>\n");
				}
			}

			definitions = m_LinkDefinitions;

			// Done
			return sb.ToString();
		}
		
		// Override to qualify non-local image and link urls
		public virtual string OnQualifyUrl(string url)
		{
			if (QualifyUrlFunc != null)
			{
				var q = QualifyUrlFunc(url);
				if (q != null)
					return q;
			}

			// Is the url a fragment?
			if (url.StartsWith("#"))
				return url;

			// Is the url already fully qualified?
			if (Utils.IsUrlFullyQualified(url))
				return url;

			if (url.StartsWith("/"))
			{
				if (!string.IsNullOrEmpty(UrlRootLocation))
				{
					return UrlRootLocation + url;
				}

				// Quit if we don't have a base location
				if (String.IsNullOrEmpty(UrlBaseLocation))
					return url;

				// Need to find domain root
				int pos = UrlBaseLocation.IndexOf("://");
				if (pos == -1)
					pos = 0;
				else
					pos += 3;

				// Find the first slash after the protocol separator
				pos = UrlBaseLocation.IndexOf('/', pos);

				// Get the domain name
				string strDomain=pos<0 ? UrlBaseLocation : UrlBaseLocation.Substring(0, pos);

				// Join em
				return strDomain + url;
			}
			else
			{
				// Quit if we don't have a base location
				if (String.IsNullOrEmpty(UrlBaseLocation))
					return url;

				if (!UrlBaseLocation.EndsWith("/"))
					return UrlBaseLocation + "/" + url;
				return UrlBaseLocation + url;
			}
		}

		// Override to supply the size of an image
		public virtual bool OnGetImageSize(string url, bool TitledImage, out int width, out int height)
		{
			if (GetImageSizeFunc != null)
			{
				var info = new ImageInfo() { url = url, titled_image=TitledImage };
				if (GetImageSizeFunc(info))
				{
					width = info.width;
					height = info.height;
					return true;
				}
			}

			width = 0;
			height = 0;

			if (Utils.IsUrlFullyQualified(url))
				return false;

			// Work out base location
			string str = url.StartsWith("/") ? DocumentRoot : DocumentLocation;
			if (String.IsNullOrEmpty(str))
				return false;

			// Work out file location
			if (str.EndsWith("/") || str.EndsWith("\\"))
			{
				str=str.Substring(0, str.Length-1);
			}

			if (url.StartsWith("/"))
			{
				url=url.Substring(1);
			}

			str=str + "\\" + url.Replace("/", "\\");


			// 

			//Create an image object from the uploaded file
			try
			{
				var img = System.Drawing.Image.FromFile(str);
				width=img.Width;
				height=img.Height;

				if (MaxImageWidth != 0 && width>MaxImageWidth)
				{
					height=(int)((double)height * (double)MaxImageWidth / (double)width);
					width=MaxImageWidth;
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		
		// Override to modify the attributes of a link
		public virtual void OnPrepareLink(HtmlTag tag)
		{
			if (PrepareLinkFunc != null)
			{
				if (PrepareLinkFunc(tag))
					return;
			}

			string url = tag.attributes["href"];

			// No follow?
			if (NoFollowLinks)
			{
				tag.attributes["rel"] = "nofollow";
			}

            // No follow external links only
            if (NoFollowExternalLinks)
            {
                if (Utils.IsUrlFullyQualified(url))
                    tag.attributes["rel"] = "nofollow";
            }
 		
			// New window?
			if ( (NewWindowForExternalLinks && Utils.IsUrlFullyQualified(url)) ||
				 (NewWindowForLocalLinks && !Utils.IsUrlFullyQualified(url)) )
			{
				tag.attributes["target"] = "_blank";
			}

			// Qualify url
			tag.attributes["href"] = OnQualifyUrl(url);
		}

		// Override to modify the attributes of an image
		public virtual void OnPrepareImage(HtmlTag tag, bool TitledImage)
		{
			if (PrepareImageFunc != null)
			{
				if (PrepareImageFunc(tag, TitledImage))
					return;
			}

			// Try to determine width and height
			int width, height;
			if (OnGetImageSize(tag.attributes["src"], TitledImage, out width, out height))
			{
				tag.attributes["width"] = width.ToString();
				tag.attributes["height"] = height.ToString();
			}

			// Now qualify the url
			tag.attributes["src"] = OnQualifyUrl(tag.attributes["src"]);
		}

		public virtual void OnSectionHeader(StringBuilder dest, int Index)
		{
			if (SectionHeader != null)
			{
				dest.AppendFormat(SectionHeader, Index);
			}
		}

		public virtual void OnSectionHeadingSuffix(StringBuilder dest, int Index)
		{
			if (SectionHeadingSuffix != null)
			{
				dest.AppendFormat(SectionHeadingSuffix, Index);
			}
		}

		public virtual void OnSectionFooter(StringBuilder dest, int Index)
		{
			if (SectionFooter!=null)
			{
				dest.AppendFormat(SectionFooter, Index);
			}
		}

		private bool IsSectionHeader(Block b)
		{
			return b.BlockType >= BlockType.h1 && b.BlockType <= BlockType.h3;
		}

		
		// Split the markdown into sections, one section for each
		// top level heading
		public static List<string> SplitUserSections(string markdown)
		{
			// Build blocks
			var md = new MarkdownDeep.Markdown();
			md.UserBreaks = true;

			// Process blocks
			var blocks = md.ProcessBlocks(markdown);

			// Create sections
			var Sections = new List<string>();
			int iPrevSectionOffset = 0;
			for (int i = 0; i < blocks.Count; i++)
			{
				var b = blocks[i];
				if (b.BlockType==BlockType.user_break)
				{
					// Get the offset of the section
					int iSectionOffset = b.LineStart;

					// Add section
					Sections.Add(markdown.Substring(iPrevSectionOffset, iSectionOffset - iPrevSectionOffset).Trim());

					// Next section starts on next line
					if (i + 1 < blocks.Count)
					{
						iPrevSectionOffset = blocks[i + 1].LineStart;
						if (iPrevSectionOffset==0)
							iPrevSectionOffset = blocks[i + 1].ContentStart;
					}
					else
						iPrevSectionOffset = markdown.Length;
				}
			}

			// Add the last section
			if (markdown.Length > iPrevSectionOffset)
			{
				Sections.Add(markdown.Substring(iPrevSectionOffset).Trim());
			}

			return Sections;
		}

		// Join previously split sections back into one document
		public static string JoinUserSections(List<string> sections)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < sections.Count; i++)
			{
				if (i > 0)
				{
					// For subsequent sections, need to make sure we
					// have a line break after the previous section.
					string strPrev = sections[sections.Count - 1];
					if (strPrev.Length > 0 && !strPrev.EndsWith("\n") && !strPrev.EndsWith("\r"))
						sb.Append("\n");

					sb.Append("\n===\n\n");
				}

				sb.Append(sections[i]);
			}

			return sb.ToString();
		}

		// Split the markdown into sections, one section for each
		// top level heading
		public static List<string> SplitSections(string markdown)
		{
			// Build blocks
			var md = new MarkdownDeep.Markdown();

			// Process blocks
			var blocks = md.ProcessBlocks(markdown);

			// Create sections
			var Sections = new List<string>();
			int iPrevSectionOffset = 0;
			for (int i = 0; i < blocks.Count; i++)
			{
				var b = blocks[i];
				if (md.IsSectionHeader(b))
				{
					// Get the offset of the section
					int iSectionOffset = b.LineStart;

					// Add section
					Sections.Add(markdown.Substring(iPrevSectionOffset, iSectionOffset - iPrevSectionOffset));

					iPrevSectionOffset = iSectionOffset;
				}
			}

			// Add the last section
			if (markdown.Length > iPrevSectionOffset)
			{
				Sections.Add(markdown.Substring(iPrevSectionOffset));
			}

			return Sections;
		}

		// Join previously split sections back into one document
		public static string JoinSections(List<string> sections)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < sections.Count; i++)
			{
				if (i > 0)
				{
					// For subsequent sections, need to make sure we
					// have a line break after the previous section.
					string strPrev = sections[sections.Count - 1];
					if (strPrev.Length>0 && !strPrev.EndsWith("\n") && !strPrev.EndsWith("\r"))
						sb.Append("\n");
				}

				sb.Append(sections[i]);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Gets the new tab identifier, which is a unique number within the markdown parsing pass.
		/// </summary>
		/// <returns></returns>
		internal int GetNewTabId()
		{
			_tabIdCounter++;
			return _tabIdCounter;
		}

		// Add a link definition
		internal void AddLinkDefinition(LinkDefinition link)
		{
			// Store it
			m_LinkDefinitions[link.Id]=link;
		}

		internal void AddFootnote(Block footnote)
		{
			m_Footnotes[(string)footnote.Data] = footnote;
		}

		// Look up a footnote, claim it and return it's index (or -1 if not found)
		internal int ClaimFootnote(string id)
		{
			Block footnote;
			if (m_Footnotes.TryGetValue(id, out footnote))
			{
				// Move the foot note to the used footnote list
				m_UsedFootnotes.Add(footnote);
				m_Footnotes.Remove(id);

				// Return it's display index
				return m_UsedFootnotes.Count-1;
			}
			else
				return -1;
		}

		// Get a link definition
		public LinkDefinition GetLinkDefinition(string id)
		{
			LinkDefinition link;
			if (m_LinkDefinitions.TryGetValue(id, out link))
				return link;
			else
				return null;
		}

		internal void AddAbbreviation(string abbr, string title)
		{
			if (m_AbbreviationMap == null)
			{
				// First time
				m_AbbreviationMap = new Dictionary<string, Abbreviation>();
			}
			else if (m_AbbreviationMap.ContainsKey(abbr))
			{
				// Remove previous
				m_AbbreviationMap.Remove(abbr);
			}

			// Store abbreviation
			m_AbbreviationMap.Add(abbr, new Abbreviation(abbr, title));

		}

		internal List<Abbreviation> GetAbbreviations()
		{
			return m_AbbreviationList;
		}

		// HtmlEncode a range in a string to a specified string builder
		internal void HtmlEncode(StringBuilder dest, string str, int start, int len)
		{
			m_StringScanner.Reset(str, start, len);
			var p = m_StringScanner;
			while (!p.Eof)
			{
				char ch = p.Current;
				switch (ch)
				{
					case '&':
						dest.Append("&amp;");
						break;

					case '<':
						dest.Append("&lt;");
						break;

					case '>':
						dest.Append("&gt;");
						break;

					case '\"':
						dest.Append("&quot;");
						break;

					default:
						dest.Append(ch);
						break;
				}
				p.SkipForward(1);
			}
		}


		// HtmlEncode a string, also converting tabs to spaces (used by CodeBlocks)
		internal void HtmlEncodeAndConvertTabsToSpaces(StringBuilder dest, string str, int start, int len)
		{
			m_StringScanner.Reset(str, start, len);
			var p = m_StringScanner;
			int pos = 0;
			while (!p.Eof)
			{
				char ch = p.Current;
				switch (ch)
				{
					case '\t':
						dest.Append(' ');
						pos++;
						while ((pos % 4) != 0)
						{
							dest.Append(' ');
							pos++;
						}
						pos--;		// Compensate for the pos++ below
						break;

					case '\r':
					case '\n':
						dest.Append('\n');
						pos = 0;
						p.SkipEol();
						continue;

					case '&':
						dest.Append("&amp;");
						break;

					case '<':
						dest.Append("&lt;");
						break;

					case '>':
						dest.Append("&gt;");
						break;

					case '\"':
						dest.Append("&quot;");
						break;

					default:
						dest.Append(ch);
						break;
				}
				p.SkipForward(1);
				pos++;
			}
		}

		internal string MakeUniqueHeaderID(string strHeaderText)
		{
			return MakeUniqueHeaderID(strHeaderText, 0, strHeaderText.Length);

		}

		internal string MakeUniqueHeaderID(string strHeaderText, int startOffset, int length)
		{
			if (!AutoHeadingIDs)
				return null;

			// Extract a pandoc style cleaned header id from the header text
			string strBase=m_SpanFormatter.MakeID(strHeaderText, startOffset, length);

			// If nothing left, use "section"
			if (String.IsNullOrEmpty(strBase))
				strBase = "section";

			// Make sure it's unique by append -n counter
			string strWithSuffix=strBase;
			int counter=1;
			while (m_UsedHeaderIDs.ContainsKey(strWithSuffix))
			{
				strWithSuffix = strBase + "-" + counter.ToString();
				counter++;
			}

			// Store it
			m_UsedHeaderIDs.Add(strWithSuffix, true);

			// Return it
			return strWithSuffix;
		}


		/*
		 * Get this markdown processors string builder.  
		 * 
		 * We re-use the same string builder whenever we can for performance.  
		 * We just reset the length before starting to / use it again, which 
		 * hopefully should keep the memory around for next time.
		 * 
		 * Note, care should be taken when using this string builder to not
		 * call out to another function that also uses it.
		 */
		internal StringBuilder GetStringBuilder()
		{
			m_StringBuilder.Length = 0;
			return m_StringBuilder;
		}

		#region Block Pooling

		// We cache and re-use blocks for performance

		Stack<Block> m_SpareBlocks=new Stack<Block>();

		internal Block CreateBlock()
		{
			if (m_SpareBlocks.Count!=0)
				return m_SpareBlocks.Pop();
			else
				return new Block();
		}

		internal void FreeBlock(Block b)
		{
			m_SpareBlocks.Push(b);
		}

		#endregion


		#region Properties
		internal bool RenderingTitledImage { get; set; }

		internal SpanFormatter SpanFormatter
		{
			get
			{
				return m_SpanFormatter;
			}
		}


		public int SummaryLength
		{
			get;
			set;
		}


		/// <summary>
		/// Set to true to enable GitHub style codeblocks, which enables GitHub style codeblocks:
		/// ```cs
		/// code
		/// ```
		/// will result in specifying the specified name after the first ``` as the class in the code element. Which can then be used with highlight.js.
		/// </summary>
		public bool GitHubCodeBlocks { get; set; }

		// Set to true to only allow whitelisted safe html tags
		public bool SafeMode
		{
			get;
			set;
		}

		// Set to true to enable ExtraMode, which enables the same set of 
		// features as implemented by PHP Markdown Extra.
		//  - Markdown in html (eg: <div markdown="1"> or <div markdown="deep"> )
		//  - Header ID attributes
		//  - Fenced code blocks
		//  - Definition lists
		//  - Footnotes
		//  - Abbreviations
		//  - Simple tables
		public bool ExtraMode
		{
			get;
			set;
		}


		/// <summary>
		/// Gets or sets a value indicating whether the parser is in DocNet mode. If set to true, it will handle '@' extensions added for DocNet as well as
		/// collect H2 id elements. 
		/// </summary>
		public bool DocNetMode { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the parser is in HnD mode. If set to true, it will handle '@' extensions from DocNet as well as specific extensions for HnD
		/// </summary>
		public bool HnDMode { get; set; }

		// When set, all html block level elements automatically support
		// markdown syntax within them.  
		// (Similar to Pandoc's handling of markdown in html)
		public bool MarkdownInHtml
		{
			get;
			set;
		}

		// When set, all headings will have an auto generated ID attribute
		// based on the heading text (uses the same algorithm as Pandoc)
		public bool AutoHeadingIDs
		{
			get;
			set;
		}

		// When set, all non-qualified urls (links and images) will
		// be qualified using this location as the base.
		// Useful when rendering RSS feeds that require fully qualified urls.
		public string UrlBaseLocation
		{
			get;
			set;
		}

		// When set, all non-qualified urls (links and images) begining with a slash
		// will qualified by prefixing with this string.
		// Useful when rendering RSS feeds that require fully qualified urls.
		public string UrlRootLocation
		{
			get;
			set;
		}

		// When true, all fully qualified urls will be give `target="_blank"' attribute
		// causing them to appear in a separate browser window/tab
		// ie: relative links open in same window, qualified links open externally
		public bool NewWindowForExternalLinks
		{
			get;
			set;
		}

		// When true, all urls (qualified or not) will get target="_blank" attribute
		// (useful for preview mode on posts)
		public bool NewWindowForLocalLinks
		{
			get;
			set;
		}

		// When set, will try to determine the width/height for local images by searching
		// for an appropriately named file relative to the specified location
		// Local file system location of the document root.  Used to locate image
		// files that start with slash.
		// Typical value: c:\inetpub\www\wwwroot
		public string DocumentRoot
		{
			get;
			set;
		}

		// Local file system location of the current document.  Used to locate relative
		// path images for image size.
		// Typical value: c:\inetpub\www\wwwroot\subfolder
		public string DocumentLocation
		{
			get;
			set;
		}

		// Limit the width of images (0 for no limit)
		public int MaxImageWidth
		{
			get;
			set;
		}

		// Set rel="nofollow" on all links
		public bool NoFollowLinks
		{
			get;
			set;
		}

		/// <summary>
		/// Add the NoFollow attribute to all external links.
		/// </summary>
		public bool NoFollowExternalLinks
		{
			get;
			set;
		}


		/// <summary>
		/// Collector for the created id's for H2 headers. First element in Tuple is id name, second is name for ToC (the text for H2). Id's are generated
		/// by the parser and use pandoc algorithm, as AutoHeadingId's is switched on. Only in use if DocNetMode is set to true
		/// </summary>
		public List<Tuple<string, string>> CreatedH2IdCollector { get; private set; }


		// Set the html class for the footnotes div
		// (defaults to "footnotes")
		// btw fyi: you can use css to disable the footnotes horizontal rule. eg:
		// div.footnotes hr { display:none }
		public string HtmlClassFootnotes
		{
			get;
			set;
		}

		public Func<string, string> QualifyUrlFunc { get; set; }
		public Func<ImageInfo, bool> GetImageSizeFunc { get; set; }
		public Func<HtmlTag, bool> PrepareLinkFunc { get; set; }
		public Func<HtmlTag, bool, bool> PrepareImageFunc { get; set; }
		// Callback to format a code block (ie: apply syntax highlighting)
		// string FormatCodeBlock(code)
		// Code = code block content (ie: the code to format)
		// Return the formatted code, including <pre> and <code> tags
		public Func<Markdown, string, string> FormatCodeBlockFunc { get; set; }

		// when set to true, will remove head blocks and make content available
		// as HeadBlockContent
		public bool ExtractHeadBlocks
		{
			get;
			set;
		}

		// Retrieve extracted head block content
		public string HeadBlockContent
		{
			get;
			internal set;
		}

		// Treats "===" as a user section break
		public bool UserBreaks
		{
			get;
			set;
		}

		// Set the classname for titled images
		// A titled image is defined as a paragraph that contains an image and nothing else.
		// If not set (the default), this features is disabled, otherwise the output is:
		// 
		// <div class="<%=this.HtmlClassTitledImags%>">
		//	<img src="image.png" />
		//	<p>Alt text goes here</p>
		// </div>
		//
		// Use CSS to style the figure and the caption
		public string HtmlClassTitledImages
		{
			get;
			set;
		}

		// Set a format string to be rendered before headings
		// {0} = section number
		// (useful for rendering links that can lead to a page that edits that section)
		// (eg: "<a href=/edit/page?section={0}>"
		public string SectionHeader
		{
			get;
			set;
		}

		// Set a format string to be rendered after each section heading
		public string SectionHeadingSuffix
		{
			get;
			set;
		}

		// Set a format string to be rendered after the section content (ie: before
		// the next section heading, or at the end of the document).
		public string SectionFooter
		{
			get;
			set;
		}


		/// <summary>
		/// Gets the sd token start end indices, which are start/end index pairs for the start tokens in the '@' token set extension for DocNet / HnD. 
		/// </summary>
		public Dictionary<int, int> SDTokenStartEndIndices
		{
			get { return _sdTokenStartEndIndices; }
		}
		#endregion
	}

}
