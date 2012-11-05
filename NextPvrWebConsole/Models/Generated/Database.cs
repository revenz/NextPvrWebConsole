
// This file was automatically generated by the PetaPoco T4 Template
// Do not make changes directly to this file - edit the template instead
// 
// The following connection settings were used to generate this file
// 
//     Connection String Name: `DefaultConnection`
//     Provider:               `System.Data.SqlClient`
//     Connection String:      `Data Source=.\SQLEXPRESS;Initial Catalog=aspnet-NextPvrWebConsole-20121029102544;Integrated Security=SSPI`
//     Schema:                 ``
//     Include Views:          `False`

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PetaPoco;

namespace DefaultConnection
{
	public partial class DefaultConnectionDB : Database
	{
		public DefaultConnectionDB() 
			: base("DefaultConnection")
		{
			CommonConstruct();
		}

		public DefaultConnectionDB(string connectionStringName) 
			: base(connectionStringName)
		{
			CommonConstruct();
		}
		
		partial void CommonConstruct();
		
		public interface IFactory
		{
			DefaultConnectionDB GetInstance();
		}
		
		public static IFactory Factory { get; set; }
        public static DefaultConnectionDB GetInstance()
        {
			if (_instance!=null)
				return _instance;
				
			if (Factory!=null)
				return Factory.GetInstance();
			else
				return new DefaultConnectionDB();
        }

		[ThreadStatic] static DefaultConnectionDB _instance;
		
		public override void OnBeginTransaction()
		{
			if (_instance==null)
				_instance=this;
		}
		
		public override void OnEndTransaction()
		{
			if (_instance==this)
				_instance=null;
		}
        
		public class Record<T> where T:new()
		{
			public static DefaultConnectionDB repo { get { return DefaultConnectionDB.GetInstance(); } }
			public bool IsNew() { return repo.IsNew(this); }
			public object Insert() { return repo.Insert(this); }
			public int Update(IEnumerable<string> columns) { return repo.Update(this, columns); }
			public static int Update(string sql, params object[] args) { return repo.Update<T>(sql, args); }
			public static int Update(Sql sql) { return repo.Update<T>(sql); }
			public int Delete() { return repo.Delete(this); }
			public static int Delete(string sql, params object[] args) { return repo.Delete<T>(sql, args); }
			public static int Delete(Sql sql) { return repo.Delete<T>(sql); }
			public static int Delete(object primaryKey) { return repo.Delete<T>(primaryKey); }
			public static bool Exists(object primaryKey) { return repo.Exists<T>(primaryKey); }
			public static T SingleOrDefault(object primaryKey) { return repo.SingleOrDefault<T>(primaryKey); }
			public static T SingleOrDefault(string sql, params object[] args) { return repo.SingleOrDefault<T>(sql, args); }
			public static T SingleOrDefault(Sql sql) { return repo.SingleOrDefault<T>(sql); }
			public static T FirstOrDefault(string sql, params object[] args) { return repo.FirstOrDefault<T>(sql, args); }
			public static T FirstOrDefault(Sql sql) { return repo.FirstOrDefault<T>(sql); }
			public static T Single(object primaryKey) { return repo.Single<T>(primaryKey); }
			public static T Single(string sql, params object[] args) { return repo.Single<T>(sql, args); }
			public static T Single(Sql sql) { return repo.Single<T>(sql); }
			public static T First(string sql, params object[] args) { return repo.First<T>(sql, args); }
			public static T First(Sql sql) { return repo.First<T>(sql); }
			public static List<T> Fetch(string sql, params object[] args) { return repo.Fetch<T>(sql, args); }
			public static List<T> Fetch(Sql sql) { return repo.Fetch<T>(sql); }
			public static List<T> Fetch(long page, long itemsPerPage, string sql, params object[] args) { return repo.Fetch<T>(page, itemsPerPage, sql, args); }
			public static List<T> Fetch(long page, long itemsPerPage, Sql sql) { return repo.Fetch<T>(page, itemsPerPage, sql); }
			public static List<T> SkipTake(long skip, long take, string sql, params object[] args) { return repo.SkipTake<T>(skip, take, sql, args); }
			public static List<T> SkipTake(long skip, long take, Sql sql) { return repo.SkipTake<T>(skip, take, sql); }
			public static Page<T> Page(long page, long itemsPerPage, string sql, params object[] args) { return repo.Page<T>(page, itemsPerPage, sql, args); }
			public static Page<T> Page(long page, long itemsPerPage, Sql sql) { return repo.Page<T>(page, itemsPerPage, sql); }
			public static IEnumerable<T> Query(string sql, params object[] args) { return repo.Query<T>(sql, args); }
			public static IEnumerable<T> Query(Sql sql) { return repo.Query<T>(sql); }
			
			private Dictionary<string,bool> ModifiedColumns;
			private void OnLoaded()
			{
				ModifiedColumns = new Dictionary<string,bool>();
			}
			protected void MarkColumnModified(string column_name)
			{
				if (ModifiedColumns!=null)
					ModifiedColumns[column_name]=true;
			}
			public int Update() 
			{ 
				if (ModifiedColumns==null)
					return repo.Update(this); 

				int retv = repo.Update(this, ModifiedColumns.Keys);
				ModifiedColumns.Clear();
				return retv;
			}
			public void Save() 
			{ 
				if (repo.IsNew(this))
					repo.Insert(this);
				else
					Update();
			}
		}
	}
	

    
	[TableName("webpages_Roles")]
	[PrimaryKey("RoleId")]
	[ExplicitColumns]
    public partial class webpages_Role : DefaultConnectionDB.Record<webpages_Role>  
    {
        [Column] 
		public int RoleId 
		{ 
			get
			{
				return _RoleId;
			}
			set
			{
				_RoleId = value;
				MarkColumnModified("RoleId");
			}
		}
		int _RoleId;

        [Column] 
		public string RoleName 
		{ 
			get
			{
				return _RoleName;
			}
			set
			{
				_RoleName = value;
				MarkColumnModified("RoleName");
			}
		}
		string _RoleName;

	}
    
	[TableName("webpages_UsersInRoles")]
	[PrimaryKey("UserId", autoIncrement=false)]
	[ExplicitColumns]
    public partial class webpages_UsersInRole : DefaultConnectionDB.Record<webpages_UsersInRole>  
    {
        [Column] 
		public int UserId 
		{ 
			get
			{
				return _UserId;
			}
			set
			{
				_UserId = value;
				MarkColumnModified("UserId");
			}
		}
		int _UserId;

        [Column] 
		public int RoleId 
		{ 
			get
			{
				return _RoleId;
			}
			set
			{
				_RoleId = value;
				MarkColumnModified("RoleId");
			}
		}
		int _RoleId;

	}
    
	[TableName("UserProfile")]
	[PrimaryKey("UserId")]
	[ExplicitColumns]
    public partial class UserProfile : DefaultConnectionDB.Record<UserProfile>  
    {
        [Column] 
		public int UserId 
		{ 
			get
			{
				return _UserId;
			}
			set
			{
				_UserId = value;
				MarkColumnModified("UserId");
			}
		}
		int _UserId;

        [Column] 
		public string UserName 
		{ 
			get
			{
				return _UserName;
			}
			set
			{
				_UserName = value;
				MarkColumnModified("UserName");
			}
		}
		string _UserName;

	}
    
	[TableName("webpages_OAuthMembership")]
	[PrimaryKey("Provider", autoIncrement=false)]
	[ExplicitColumns]
    public partial class webpages_OAuthMembership : DefaultConnectionDB.Record<webpages_OAuthMembership>  
    {
        [Column] 
		public string Provider 
		{ 
			get
			{
				return _Provider;
			}
			set
			{
				_Provider = value;
				MarkColumnModified("Provider");
			}
		}
		string _Provider;

        [Column] 
		public string ProviderUserId 
		{ 
			get
			{
				return _ProviderUserId;
			}
			set
			{
				_ProviderUserId = value;
				MarkColumnModified("ProviderUserId");
			}
		}
		string _ProviderUserId;

        [Column] 
		public int UserId 
		{ 
			get
			{
				return _UserId;
			}
			set
			{
				_UserId = value;
				MarkColumnModified("UserId");
			}
		}
		int _UserId;

	}
    
	[TableName("webpages_Membership")]
	[PrimaryKey("UserId", autoIncrement=false)]
	[ExplicitColumns]
    public partial class webpages_Membership : DefaultConnectionDB.Record<webpages_Membership>  
    {
        [Column] 
		public int UserId 
		{ 
			get
			{
				return _UserId;
			}
			set
			{
				_UserId = value;
				MarkColumnModified("UserId");
			}
		}
		int _UserId;

        [Column] 
		public DateTime? CreateDate 
		{ 
			get
			{
				return _CreateDate;
			}
			set
			{
				_CreateDate = value;
				MarkColumnModified("CreateDate");
			}
		}
		DateTime? _CreateDate;

        [Column] 
		public string ConfirmationToken 
		{ 
			get
			{
				return _ConfirmationToken;
			}
			set
			{
				_ConfirmationToken = value;
				MarkColumnModified("ConfirmationToken");
			}
		}
		string _ConfirmationToken;

        [Column] 
		public bool? IsConfirmed 
		{ 
			get
			{
				return _IsConfirmed;
			}
			set
			{
				_IsConfirmed = value;
				MarkColumnModified("IsConfirmed");
			}
		}
		bool? _IsConfirmed;

        [Column] 
		public DateTime? LastPasswordFailureDate 
		{ 
			get
			{
				return _LastPasswordFailureDate;
			}
			set
			{
				_LastPasswordFailureDate = value;
				MarkColumnModified("LastPasswordFailureDate");
			}
		}
		DateTime? _LastPasswordFailureDate;

        [Column] 
		public int PasswordFailuresSinceLastSuccess 
		{ 
			get
			{
				return _PasswordFailuresSinceLastSuccess;
			}
			set
			{
				_PasswordFailuresSinceLastSuccess = value;
				MarkColumnModified("PasswordFailuresSinceLastSuccess");
			}
		}
		int _PasswordFailuresSinceLastSuccess;

        [Column] 
		public string Password 
		{ 
			get
			{
				return _Password;
			}
			set
			{
				_Password = value;
				MarkColumnModified("Password");
			}
		}
		string _Password;

        [Column] 
		public DateTime? PasswordChangedDate 
		{ 
			get
			{
				return _PasswordChangedDate;
			}
			set
			{
				_PasswordChangedDate = value;
				MarkColumnModified("PasswordChangedDate");
			}
		}
		DateTime? _PasswordChangedDate;

        [Column] 
		public string PasswordSalt 
		{ 
			get
			{
				return _PasswordSalt;
			}
			set
			{
				_PasswordSalt = value;
				MarkColumnModified("PasswordSalt");
			}
		}
		string _PasswordSalt;

        [Column] 
		public string PasswordVerificationToken 
		{ 
			get
			{
				return _PasswordVerificationToken;
			}
			set
			{
				_PasswordVerificationToken = value;
				MarkColumnModified("PasswordVerificationToken");
			}
		}
		string _PasswordVerificationToken;

        [Column] 
		public DateTime? PasswordVerificationTokenExpirationDate 
		{ 
			get
			{
				return _PasswordVerificationTokenExpirationDate;
			}
			set
			{
				_PasswordVerificationTokenExpirationDate = value;
				MarkColumnModified("PasswordVerificationTokenExpirationDate");
			}
		}
		DateTime? _PasswordVerificationTokenExpirationDate;

	}
}


