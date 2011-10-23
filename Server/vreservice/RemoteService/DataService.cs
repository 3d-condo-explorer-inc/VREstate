﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using Vre.Server.BusinessLogic;
using Vre.Server.BusinessLogic.Client;
//using Vre.Server.Dao;

namespace Vre.Server.RemoteService
{
    internal class DataService
    {
        public const string ServicePathPrefix = ServicePathElement0 + "/";
        private const string ServicePathElement0 = "data";

        enum ModelObject { User, EstateDeveloper, Site, Building, Suite, SuiteType }

        public static void ProcessGetRequest(IServiceRequest request)
        {
            ModelObject mo;
            int objectId;

            getPathElements(request.Request.Path, out mo, out objectId);

            switch (mo)
            {
                case ModelObject.Building:
                    if (-1 == objectId)
                    {
                        int siteId = request.Request.Query.GetParam("site", -1);
                        if (-1 == siteId) throw new ArgumentException("Site ID is missing.");
                        getBuildingList(request.UserInfo.Session, siteId, request.Response);
                    }
                    else
                    {
                        // TODO: get a single building data
                        throw new NotImplementedException();
                    }
                    return;

                case ModelObject.Suite:
                    if (-1 == objectId)
                    {
                        int buildingId = request.Request.Query.GetParam("building", -1);
                        if (-1 == buildingId) throw new ArgumentException("Building ID is missing.");
                        getSuiteList(request.UserInfo.Session, buildingId, request.Response);
                    }
                    else
                    {
                        getSuite(request.UserInfo.Session, objectId, request.Response);
                    }
                    return;

                case ModelObject.SuiteType:
                    if (-1 == objectId)
                    {
                        int siteId = request.Request.Query.GetParam("site", -1);
                        if (-1 == siteId) throw new ArgumentException("Site ID is missing.");
                        getSuiteTypeList(request.UserInfo.Session, siteId, request.Response);
                    }
                    else
                    {
                        // TODO: get a single suite type data
                        throw new NotImplementedException();
                    }
                    return;

            }

            throw new NotImplementedException();
        }

        public static void ProcessReplaceRequest(IServiceRequest request)
        {
            ModelObject mo;
            int objectId;

            getPathElements(request.Request.Path, out mo, out objectId);
            if (-1 == objectId) throw new ArgumentException("Object ID is missing.");

            if (null == request.Request.Data) throw new ArgumentException("Object data not passed.");

            switch (mo)
            {
                case ModelObject.Building:
                    updateBuilding(request.UserInfo.Session, objectId, request.Request.Data, request.Response);
                    return;

                case ModelObject.User:
                    updateUser(request.UserInfo.Session, request.Request.Data, request.Response);
                    return;
            }

            throw new NotImplementedException();
        }

        public static void ProcessCreateRequest(IServiceRequest request)
        {
            ModelObject mo;
            int objectId;

            getPathElements(request.Request.Path, out mo, out objectId);

            if (null == request.Request.Data) throw new ArgumentException("Object data not passed.");

            switch (mo)
            {
                case ModelObject.User:
                    createUser(request.UserInfo.Session, request.Request.Data, request.Response);
                    return;
            }

            throw new NotImplementedException();
        }

        public static void ProcessDeleteRequest(IServiceRequest request)
        {
            ModelObject mo;
            int objectId;

            getPathElements(request.Request.Path, out mo, out objectId);
            if (-1 == objectId) throw new ArgumentException("Object ID is missing.");

            switch (mo)
            {
                case ModelObject.User:
                    deleteUser(request.UserInfo.Session, objectId, request.Response);
                    return;
            }

            throw new NotImplementedException();
        }

        private static void getPathElements(string path, out ModelObject mo, out int id)
        {
            string[] elements = path.Split('/');
            if ((elements.Length < 2) || (elements.Length > 3)) throw new ArgumentException("Object path is invalid (0).");

            if (!elements[0].Equals(ServicePathElement0)) throw new ArgumentException("Object path is invalid (1).");

            if (elements[1].Equals("ed")) mo = ModelObject.EstateDeveloper;
            else if (elements[1].Equals("site")) mo = ModelObject.Site;
            else if (elements[1].Equals("building")) mo = ModelObject.Building;
            else if (elements[1].Equals("suite")) mo = ModelObject.Suite;
            else if (elements[1].Equals("user")) mo = ModelObject.User;
            else if (elements[1].Equals("suitetype")) mo = ModelObject.SuiteType;
            else throw new ArgumentException("Object path is invalid (2).");

            if (2 == elements.Length)
            {
                id = -1;
            }
            else
            {
                if (!int.TryParse(elements[2], out id)) throw new ArgumentException("Object path is invalid (3).");
            }
        }

        #region retrieval
        private static void getBuildingList(ClientSession session, int siteId, IResponseData resp)
        {
            Building[] buildingList;

            using (SiteManager manager = new SiteManager(session))
            {
                buildingList = manager.ListBuildings(siteId);
            }

            foreach (Building b in buildingList) ServiceInstances.ModelCache.FillWithModelInfo(b, false);

            // produce output
            //
            int cnt = buildingList.Length;
            ClientData[] buildings = new ClientData[cnt];
            for (int idx = 0; idx < cnt; idx++) buildings[idx] = buildingList[idx].GetClientData();

            resp.Data = new ClientData();
            resp.Data.Add("buildings", buildings);
            resp.ResponseCode = HttpStatusCode.OK;
        }

        private static void getSuiteList(ClientSession session, int buildingId, IResponseData resp)
        {
            Suite[] suiteList;
            ClientData[] suites;

            using (SiteManager manager = new SiteManager(session))
            {
                suiteList = manager.ListSuitesByBuiding(buildingId);

                foreach (Suite s in suiteList) ServiceInstances.ModelCache.FillWithModelInfo(s, false);

                // produce output
                //
                int cnt = suiteList.Length;
                suites = new ClientData[cnt];
                for (int idx = 0; idx < cnt; idx++)
                {
                    Suite s = suiteList[idx];
                    //ClientData cd = s.GetClientData();
                    //cd.Add("currentPrice", manager.GetCurrentSuitePrice(s));
                    //suites[idx] = cd;
                    //suites[idx] = new SuiteEx(s, manager.GetCurrentSuitePrice(s)).GetClientData();
                    suites[idx] = SuiteEx.GetClientData(s, manager.GetCurrentSuitePrice(s));
                }
            }

            resp.Data = new ClientData();
            resp.Data.Add("suites", suites);
            resp.ResponseCode = HttpStatusCode.OK;
        }

        private static void getSuite(ClientSession session, int suiteId, IResponseData resp)
        {
            Suite suite;

            using (SiteManager manager = new SiteManager(session))
            {
                suite = manager.GetSuiteById(suiteId);
            }

            ServiceInstances.ModelCache.FillWithModelInfo(suite, false);

            // produce output
            //
            resp.Data = suite.GetClientData();
            resp.ResponseCode = HttpStatusCode.OK;
        }

        private static void getSuiteType(ClientSession session, int siteId, string name, IResponseData resp)
        {
            SuiteType st;

            using (SiteManager manager = new SiteManager(session))
            {
                st = manager.GetSuiteTypeByName(siteId, name);
            }

            // TODO ?!
            //ServiceInstances.ModelCache.FillWithModelInfo(suite, false);

            // produce output
            //
            resp.Data = st.GetClientData();
            resp.ResponseCode = HttpStatusCode.OK;
        }

        private static void getSuiteTypeList(ClientSession session, int siteId, IResponseData resp)
        {
            SuiteType[] list;

            using (SiteManager manager = new SiteManager(session))
            {
                list = manager.ListSuiteTypes(siteId);
            }

            // TODO ?!
            //foreach (SuiteType st in list) ServiceInstances.ModelCache.FillWithModelInfo(b, false);

            // produce output
            //
            int cnt = list.Length;
            ClientData[] result = new ClientData[cnt];
            for (int idx = 0; idx < cnt; idx++) result[idx] = list[idx].GetClientData();

            resp.Data = new ClientData();
            resp.Data.Add("suiteTypes", result);
            resp.ResponseCode = HttpStatusCode.OK;
        }

        private static void getUser(ClientSession session, int userId, IResponseData resp)
        {
            User user;

            using (UserManager manager = new UserManager(session))
            {
                user = manager.GetUser(userId);
            }
            
            resp.Data = user.GetClientData();
            resp.ResponseCode = HttpStatusCode.OK;
        }
        #endregion

        #region update
        private static void updateBuilding(ClientSession session, int buildingId, ClientData data, IResponseData resp)
        {
            int updatedCnt = 0;
            List<int> staleIds = new List<int>();
            string error = null;
            Building building;

            using (INonNestedTransaction tran = NHibernateHelper.OpenNonNestedTransaction(session))
            {
                using (SiteManager manager = new SiteManager(session))
                {
                    building = manager.GetBuildingById(buildingId);

                    // this shall throw out if user has no right to modify building info
                    manager.TestUserCanUpdate(building);

                    foreach (ClientData suiteData in data.GetNextLevelDataArray("suites"))
                    {
                        Suite suite = null;

                        int suiteId = suiteData.GetProperty("id", -1);
                        if (suiteId >= 0)
                            try
                            {
                                suite = manager.GetSuiteById(suiteId);
                            }
                            catch (FileNotFoundException)
                            {
                                // try searching by name (number)
                                string name = suiteData.GetProperty("name", string.Empty);
                                var suites = from s in building.Suites 
                                             where s.SuiteName.Equals(name)
                                             select s;
                                if (1 == suites.Count()) suite = suites.First();
                            }

                        if (null == suite) continue;
                        if (suite.Building.AutoID != building.AutoID)
                        {
                            throw new InvalidDataException("The suite ID=" + suite.AutoID + 
                                " does not belong to building ID=" + building.AutoID);
                        }

                        bool updated = false, canUpdate = true;
                        if (suite.UpdateFromClient(suiteData))
                        {
                            try
                            {
                                if (manager.UpdateSuite(suite))
                                {
                                    updatedCnt++;
                                    updated = true;
                                }
                                else
                                {
                                    staleIds.Add(suite.AutoID);
                                    error = "At least one object is stale.";
                                    canUpdate = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                error = string.Format("Cannot update suite {0} (ID={1}): {2}", suite.SuiteName, suite.AutoID, ex.Message);
                                ServiceInstances.Logger.Error("Cannot update suite ID={0}: {1}", suite.AutoID, Utilities.ExplodeException(ex));
                                break;
                            }
                        }

                        // update price
                        if (canUpdate)
                        {
                            SuiteEx suiteEx = new SuiteEx(suite, manager.GetCurrentSuitePrice(suite));
                            if (suiteEx.UpdateFromClient(suiteData))
                            {
                                if (manager.SetSuitePrice(suite, (float)suiteEx.CurrentPrice))
                                {
                                    if (!updated) updatedCnt++;  // increment counter if not done above
                                }
                                else
                                {
                                    staleIds.Add(suite.AutoID);
                                    error = "At least one object is stale.";
                                }
                            }
                        }
                        
                        //double pp = manager.GetCurrentSuitePrice(suite);
                        //double ip = suiteData.GetProperty("currentPrice", -1.0);
                        //if ((ip > 0.0) && (ip != pp) && canUpdate)
                        //{
                        //    manager.SetSuitePrice(session.User, suite, (float)ip);
                        //    if (!updated) updatedCnt++;
                        //}
                    }  // foreach suite

                }  // using SiteManager

                if (null == error) tran.Commit();
                else tran.Rollback();
            }  // transaction

            // make sure building information is refreshed
            session.DbSession.Refresh(building);

            if (null == error)
            {
                resp.ResponseCode = (0 == updatedCnt) ? HttpStatusCode.NotModified : HttpStatusCode.OK;
                resp.Data = new ClientData();
                resp.Data.Add("updated", updatedCnt);
            }
            else
            {
                resp.ResponseCode = HttpStatusCode.Conflict;
                resp.ResponseCodeDescription = error;
                resp.Data = new ClientData();
                resp.Data.Add("updated", 0);
                if (staleIds.Count > 0) resp.Data.Add("staleIds", Utilities.ToCsv<int>(staleIds));
            }
        }

        private static void updateUser(ClientSession session, ClientData data, IResponseData resp)
        {
            int userId = data.GetProperty("id", -1);

            using (UserManager manager = new UserManager(session))
            {
                string errorReason;
                User user = manager.GetUser(userId);

                if (user.UpdateFromClient(data))
                {
                    if (manager.UpdateUser(user, out errorReason))
                    {
                        resp.ResponseCode = HttpStatusCode.OK;
                        resp.Data = new ClientData();
                        resp.Data.Add("updated", 1);
                    }
                    else
                    {
                        resp.ResponseCode = HttpStatusCode.Conflict;
                        resp.ResponseCodeDescription = errorReason;
                        resp.Data = new ClientData();
                        resp.Data.Add("updatedObject", user.GetClientData());
                    }
                }
                else
                {
                    resp.ResponseCode = HttpStatusCode.NotModified;
                    resp.Data = new ClientData();
                    resp.Data.Add("updated", 0);
                }
            }
        }
        #endregion

        #region create
        private static void createUser(ClientSession session, ClientData data, IResponseData resp)
        {
            User.Role role = data.GetProperty<User.Role>("role", User.Role.Buyer);
            LoginType type = data.GetProperty<LoginType>("type", LoginType.Plain);
            int estateDeveloperId = data.GetProperty("estateDeveloperId", -1);
            string login = data.GetProperty("login", string.Empty);
            string password = data.GetProperty("password", string.Empty);

            using (UserManager manager = new UserManager(session))
            {
                string errorReason;
                if (manager.CreateUser(role, estateDeveloperId, type, login, password, out errorReason))
                {
                    try
                    {
                        // create contact info block with any added fields from inbound JSON
                        User u = manager.GetUser(type, login);
                        u.UpdateFromClient(data);
                        resp.ResponseCode = HttpStatusCode.OK;
                    }
                    catch (Exception ex)
                    {
                        resp.ResponseCode = HttpStatusCode.Created;
                        resp.ResponseCodeDescription = "Contact information was not stored.";
                        ServiceInstances.Logger.Error("Contact information for created user {0}[{1}] was not saved: {2}", type, login, ex);
                    }
                }
                else
                {
                    resp.ResponseCode = HttpStatusCode.Forbidden;
                    resp.ResponseCodeDescription = errorReason;
                }
            }
        }
        #endregion

        #region delete
        private static void deleteUser(ClientSession session, int userId, IResponseData resp)
        {
            User user = null;
            string errorReason;

            using (UserManager manager = new UserManager(session))
            {
                user = manager.GetUser(userId);

                if (manager.DeleteUser(user, out errorReason))
                {
                    resp.ResponseCode = HttpStatusCode.OK;
                }
                else
                {
                    resp.ResponseCode = HttpStatusCode.Forbidden;
                    resp.ResponseCodeDescription = errorReason;
                    resp.Data = new ClientData();
                    resp.Data.Add("updatedObject", user.GetClientData());
                }
            }
        }
        #endregion
    }
}