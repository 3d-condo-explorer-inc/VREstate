using System;
using System.Collections.Generic;

namespace Vre.Server.BusinessLogic
{
    // About object locking: http://ayende.com/Blog/archive/2009/04/15/nhibernate-mapping-concurrency.aspx

	[Serializable]
	public partial class User : UpdateableBase
	{
	    public enum Role : byte
	    {
            Buyer = 0,
            SuperAdmin = 1,
            DeveloperAdmin = 2,
            Subcontractor = 3,
            SalesPerson = 4,
            SellingAgent = 5,
            Visitor = 6,
            Kiosk = 7
	    }

        public Role UserRole { get; private set; }
        public int? EstateDeveloperID { get; private set; }
        public ContactInfo PersonalInfo { get; private set; }

        private User() { }

		public User(int? estateDeveloperId, Role role)
		{
            InitializeNew();
            UserRole = role;
            EstateDeveloperID = estateDeveloperId;
            PersonalInfo = new ContactInfo();
        }

        /// <summary>
        /// True if this role requires valid Estate Developer association.
        /// If false, NO Estate Developer must be associated with this user.
        /// </summary>
        public static bool IsEstateDeveloperTied(Role role)
        {
            return ((role != Role.SellingAgent) && (role != Role.SuperAdmin) && (role != Role.Buyer) && (role != Role.Visitor));
        }

        public void UpdatePersonalInfo(ContactInfo info)
        {
            PersonalInfo = info;
        }

        public override ClientData GetClientData()
        {
            ClientData result = new ClientData();

            result.Add("id", AutoID);  // informational only
            result.Add("deleted", Deleted);  // informational only

            result.Add("estateDeveloperId", EstateDeveloperID);  // informational only
            result.Add("role", UserRole);  // informational only

            if (PersonalInfo != null)
                result.Add("personalInfo", PersonalInfo.GetClientData());

            return result;
        }

        public override bool UpdateFromClient(ClientData data)
        {
            bool changed = false;

            ClientData picd = data.GetNextLevelDataItem("personalInfo");
            if (picd.Count > 0)
            {
                if (null == PersonalInfo) PersonalInfo = new ContactInfo();
                if (PersonalInfo.UpdateFromClient(data.GetNextLevelDataItem("personalInfo"))) changed = true;
            }

            return changed;
        }
    }
}
