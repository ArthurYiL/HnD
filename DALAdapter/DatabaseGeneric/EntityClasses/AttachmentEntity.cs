﻿//////////////////////////////////////////////////////////////
// <auto-generated>This code was generated by LLBLGen Pro 5.7.</auto-generated>
//////////////////////////////////////////////////////////////
// Code is generated on: 
// Code is generated using templates: SD.TemplateBindings.SharedTemplates
// Templates vendor: Solutions Design.
//////////////////////////////////////////////////////////////
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using SD.HnD.DALAdapter.HelperClasses;
using SD.HnD.DALAdapter.FactoryClasses;
using SD.HnD.DALAdapter.RelationClasses;

using SD.LLBLGen.Pro.ORMSupportClasses;

namespace SD.HnD.DALAdapter.EntityClasses
{
	// __LLBLGENPRO_USER_CODE_REGION_START AdditionalNamespaces
	// __LLBLGENPRO_USER_CODE_REGION_END
	/// <summary>Entity class which represents the entity 'Attachment'.<br/><br/></summary>
	[Serializable]
	public partial class AttachmentEntity : CommonEntityBase
		// __LLBLGENPRO_USER_CODE_REGION_START AdditionalInterfaces
		// __LLBLGENPRO_USER_CODE_REGION_END	
	{
		private MessageEntity _belongsToMessage;

		// __LLBLGENPRO_USER_CODE_REGION_START PrivateMembers
		// __LLBLGENPRO_USER_CODE_REGION_END
		private static AttachmentEntityStaticMetaData _staticMetaData = new AttachmentEntityStaticMetaData();
		private static AttachmentRelations _relationsFactory = new AttachmentRelations();

		/// <summary>All names of fields mapped onto a relation. Usable for in-memory filtering</summary>
		public static partial class MemberNames
		{
			/// <summary>Member name BelongsToMessage</summary>
			public static readonly string BelongsToMessage = "BelongsToMessage";
		}

		/// <summary>Static meta-data storage for navigator related information</summary>
		protected class AttachmentEntityStaticMetaData : EntityStaticMetaDataBase
		{
			public AttachmentEntityStaticMetaData()
			{
				SetEntityCoreInfo("AttachmentEntity", InheritanceHierarchyType.None, false, (int)SD.HnD.DALAdapter.EntityType.AttachmentEntity, typeof(AttachmentEntity), typeof(AttachmentEntityFactory), false);
				AddNavigatorMetaData<AttachmentEntity, MessageEntity>("BelongsToMessage", "Attachments", (a, b) => a._belongsToMessage = b, a => a._belongsToMessage, (a, b) => a.BelongsToMessage = b, SD.HnD.DALAdapter.RelationClasses.StaticAttachmentRelations.MessageEntityUsingMessageIDStatic, ()=>new AttachmentRelations().MessageEntityUsingMessageID, null, new int[] { (int)AttachmentFieldIndex.MessageID }, null, true, (int)SD.HnD.DALAdapter.EntityType.MessageEntity);
			}
		}

		/// <summary>Static ctor</summary>
		static AttachmentEntity()
		{
		}

		/// <summary> CTor</summary>
		public AttachmentEntity()
		{
			InitClassEmpty(null, null);
		}

		/// <summary> CTor</summary>
		/// <param name="fields">Fields object to set as the fields for this entity.</param>
		public AttachmentEntity(IEntityFields2 fields)
		{
			InitClassEmpty(null, fields);
		}

		/// <summary> CTor</summary>
		/// <param name="validator">The custom validator object for this AttachmentEntity</param>
		public AttachmentEntity(IValidator validator)
		{
			InitClassEmpty(validator, null);
		}

		/// <summary> CTor</summary>
		/// <param name="attachmentID">PK value for Attachment which data should be fetched into this Attachment object</param>
		public AttachmentEntity(System.Int32 attachmentID) : this(attachmentID, null)
		{
		}

		/// <summary> CTor</summary>
		/// <param name="attachmentID">PK value for Attachment which data should be fetched into this Attachment object</param>
		/// <param name="validator">The custom validator object for this AttachmentEntity</param>
		public AttachmentEntity(System.Int32 attachmentID, IValidator validator)
		{
			InitClassEmpty(validator, null);
			this.AttachmentID = attachmentID;
		}

		/// <summary>Private CTor for deserialization</summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected AttachmentEntity(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			// __LLBLGENPRO_USER_CODE_REGION_START DeserializationConstructor
			// __LLBLGENPRO_USER_CODE_REGION_END
		}

		/// <summary>Creates a new IRelationPredicateBucket object which contains the predicate expression and relation collection to fetch the related entity of type 'Message' to this entity.</summary>
		/// <returns></returns>
		public virtual IRelationPredicateBucket GetRelationInfoBelongsToMessage() { return CreateRelationInfoForNavigator("BelongsToMessage"); }
		
		/// <inheritdoc/>
		protected override EntityStaticMetaDataBase GetEntityStaticMetaData() {	return _staticMetaData; }

		/// <summary>Initializes the class members</summary>
		private void InitClassMembers()
		{
			PerformDependencyInjection();
			// __LLBLGENPRO_USER_CODE_REGION_START InitClassMembers
			// __LLBLGENPRO_USER_CODE_REGION_END
			OnInitClassMembersComplete();
		}

		/// <summary>Initializes the class with empty data, as if it is a new Entity.</summary>
		/// <param name="validator">The validator object for this AttachmentEntity</param>
		/// <param name="fields">Fields of this entity</param>
		private void InitClassEmpty(IValidator validator, IEntityFields2 fields)
		{
			OnInitializing();
			this.Fields = fields ?? CreateFields();
			this.Validator = validator;
			InitClassMembers();
			// __LLBLGENPRO_USER_CODE_REGION_START InitClassEmpty
			// __LLBLGENPRO_USER_CODE_REGION_END

			OnInitialized();
		}

		/// <summary>The relations object holding all relations of this entity with other entity classes.</summary>
		public static AttachmentRelations Relations { get { return _relationsFactory; } }

		/// <summary>Creates a new PrefetchPathElement2 object which contains all the information to prefetch the related entities of type 'Message' for this entity.</summary>
		/// <returns>Ready to use IPrefetchPathElement2 implementation.</returns>
		public static IPrefetchPathElement2 PrefetchPathBelongsToMessage { get { return _staticMetaData.GetPrefetchPathElement("BelongsToMessage", CommonEntityBase.CreateEntityCollection<MessageEntity>()); } }

		/// <summary>The AttachmentID property of the Entity Attachment<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Attachment"."AttachmentID".<br/>Table field type characteristics (type, precision, scale, length): Int, 10, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, true, true</remarks>
		public virtual System.Int32 AttachmentID
		{
			get { return (System.Int32)GetValue((int)AttachmentFieldIndex.AttachmentID, true); }
			set { SetValue((int)AttachmentFieldIndex.AttachmentID, value); }		}

		/// <summary>The MessageID property of the Entity Attachment<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Attachment"."MessageID".<br/>Table field type characteristics (type, precision, scale, length): Int, 10, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.Int32 MessageID
		{
			get { return (System.Int32)GetValue((int)AttachmentFieldIndex.MessageID, true); }
			set	{ SetValue((int)AttachmentFieldIndex.MessageID, value); }
		}

		/// <summary>The Filename property of the Entity Attachment<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Attachment"."Filename".<br/>Table field type characteristics (type, precision, scale, length): NVarChar, 0, 0, 255.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.String Filename
		{
			get { return (System.String)GetValue((int)AttachmentFieldIndex.Filename, true); }
			set	{ SetValue((int)AttachmentFieldIndex.Filename, value); }
		}

		/// <summary>The Approved property of the Entity Attachment<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Attachment"."Approved".<br/>Table field type characteristics (type, precision, scale, length): Bit, 0, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.Boolean Approved
		{
			get { return (System.Boolean)GetValue((int)AttachmentFieldIndex.Approved, true); }
			set	{ SetValue((int)AttachmentFieldIndex.Approved, value); }
		}

		/// <summary>The Filecontents property of the Entity Attachment<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Attachment"."Filecontents".<br/>Table field type characteristics (type, precision, scale, length): Image, 0, 0, 2147483647.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.Byte[] Filecontents
		{
			get { return (System.Byte[])GetValue((int)AttachmentFieldIndex.Filecontents, true); }
			set	{ SetValue((int)AttachmentFieldIndex.Filecontents, value); }
		}

		/// <summary>The Filesize property of the Entity Attachment<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Attachment"."Filesize".<br/>Table field type characteristics (type, precision, scale, length): Int, 10, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.Int32 Filesize
		{
			get { return (System.Int32)GetValue((int)AttachmentFieldIndex.Filesize, true); }
			set	{ SetValue((int)AttachmentFieldIndex.Filesize, value); }
		}

		/// <summary>The AddedOn property of the Entity Attachment<br/><br/></summary>
		/// <remarks>Mapped on  table field: "Attachment"."AddedOn".<br/>Table field type characteristics (type, precision, scale, length): DateTime, 0, 0, 0.<br/>Table field behavior characteristics (is nullable, is PK, is identity): false, false, false</remarks>
		public virtual System.DateTime AddedOn
		{
			get { return (System.DateTime)GetValue((int)AttachmentFieldIndex.AddedOn, true); }
			set	{ SetValue((int)AttachmentFieldIndex.AddedOn, value); }
		}

		/// <summary>Gets / sets related entity of type 'MessageEntity' which has to be set using a fetch action earlier. If no related entity is set for this property, null is returned..<br/><br/></summary>
		[Browsable(true)]
		public virtual MessageEntity BelongsToMessage
		{
			get { return _belongsToMessage; }
			set { SetSingleRelatedEntityNavigator(value, "BelongsToMessage"); }
		}

		// __LLBLGENPRO_USER_CODE_REGION_START CustomEntityCode
		// __LLBLGENPRO_USER_CODE_REGION_END

	}
}

namespace SD.HnD.DALAdapter
{
	public enum AttachmentFieldIndex
	{
		///<summary>AttachmentID. </summary>
		AttachmentID,
		///<summary>MessageID. </summary>
		MessageID,
		///<summary>Filename. </summary>
		Filename,
		///<summary>Approved. </summary>
		Approved,
		///<summary>Filecontents. </summary>
		Filecontents,
		///<summary>Filesize. </summary>
		Filesize,
		///<summary>AddedOn. </summary>
		AddedOn,
		/// <summary></summary>
		AmountOfFields
	}
}

namespace SD.HnD.DALAdapter.RelationClasses
{
	/// <summary>Implements the relations factory for the entity: Attachment. </summary>
	public partial class AttachmentRelations: RelationFactory
	{

		/// <summary>Returns a new IEntityRelation object, between AttachmentEntity and MessageEntity over the m:1 relation they have, using the relation between the fields: Attachment.MessageID - Message.MessageID</summary>
		public virtual IEntityRelation MessageEntityUsingMessageID
		{
			get	{ return ModelInfoProviderSingleton.GetInstance().CreateRelation(RelationType.ManyToOne, "BelongsToMessage", false, new[] { MessageFields.MessageID, AttachmentFields.MessageID }); }
		}

	}
	
	/// <summary>Static class which is used for providing relationship instances which are re-used internally for syncing</summary>
	internal static class StaticAttachmentRelations
	{
		internal static readonly IEntityRelation MessageEntityUsingMessageIDStatic = new AttachmentRelations().MessageEntityUsingMessageID;

		/// <summary>CTor</summary>
		static StaticAttachmentRelations() { }
	}
}
