using System;
using System.IO;
using System.Web;
using System.Net;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using VI.Base;
using VI.DB;
using VI.DB.Entities;
using QBM.CompositionApi.ApiManager;
using QBM.CompositionApi.Definition;
using QBM.CompositionApi.Crud;
using QER.CompositionApi.Portal;


namespace QBM.CompositionApi
{
    public class GetPeopleManaged : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("example/identitiesmanagedbytheuser")
                .HandleGet(async (qr, ct) =>
                {
                    var loggedinUser = qr.Session.User().Uid;
                    var query = Query.From("Person")
                        .Select("FirstName", "LastName", "UID_Person")
                        .Where(String.Format(@"UID_Person IN (SELECT UID_Person FROM People WHERE UID_PersonHead = '{0}')", loggedinUser));

                    var CollGet = await qr.Session.Source().GetCollectionAsync(query, EntityCollectionLoadType.Default, ct).ConfigureAwait(false);
                    return await ReturnedName.fromEntity(CollGet, qr.Session).ConfigureAwait(false);
                }));
        }

        public class ReturnedName
        {
            
            public string uidperson { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Department { get; set; }
            public string HeadOfDepartment { get; set; }


            
            public static async Task<List<ReturnedName>> fromEntity(IEntityCollection entityCollection, ISession session)
            {
                var nameList = new List<ReturnedName>();

                foreach (var entity in entityCollection)
                {
                    var uidPerson = await entity.GetValueAsync<string>("UID_Person").ConfigureAwait(false);
                    IEntity dbPerson = await session.Source().GetAsync("Person" , uidPerson).ConfigureAwait(false);

                    var dep = dbPerson.CreateWalker(session).GetValue("FK(UID_Department).DepartmentName");
                    var hod = dbPerson.CreateWalker(session).GetValue("FK(UID_Department).FK(UID_Department).InternalName");

                    var g = new ReturnedName
                    {
                        uidperson = uidPerson,
                        FirstName = await entity.GetValueAsync<string>("FirstName").ConfigureAwait(false),
                        LastName = await entity.GetValueAsync<string>("LastName").ConfigureAwait(false),
                        Department = dep,
                        HeadOfDepartment = hod,
                    };

                    nameList.Add(g);
                }


                return nameList;
            }
        }
    }
}
