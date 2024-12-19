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
    public class PostCreateIdentity  : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("example/createidentity")
                .Handle<PostedID>("POST", async (posted, qr, ct) =>
                {
                    string firstname = "";
                    string lastname = "";
                    var loggedinuser = qr.Session.User().Uid;

                    foreach (var column in posted.columns)
                    {
                        if (column.column == "First Name")
                        {
                            firstname = column.value;
                        }

                        if (column.column == "Last Name")
                        {
                            lastname = column.value;
                        }
                    }

                    var newID = await qr.Session.Source().CreateNewAsync("Person",
                        new EntityParameters
                        {
                            CreationType = EntityCreationType.DelayedLogic
                        }, ct).ConfigureAwait(false);

                    await newID.PutValueAsync("FirstName", firstname, ct).ConfigureAwait(false);
                    await newID.PutValueAsync("LastName", lastname, ct).ConfigureAwait(false);
                    await newID.PutValueAsync("UID_PersonHead", loggedinuser, ct).ConfigureAwait(false);

                    using (var u = qr.Session.StartUnitOfWork())
                    {
                        await u.PutAsync(newID, ct).ConfigureAwait(false);
                        await u.CommitAsync(ct).ConfigureAwait(false);
                    }
                }));
        }

        public class PostedID
        {
            public columnsarray[] columns { get; set; }
        }

        public class columnsarray
        {
            public string column { get; set; }
            public string value { get; set; }
        }
    }

}
