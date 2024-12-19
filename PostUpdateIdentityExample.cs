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
    public class PostUpdateIdentityExample : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        public void Build(IApiBuilder builder)
        {
            builder.AddMethod(Method.Define("example/updateidentity")
                .Handle<PostedID>("POST", async (posted, qr, ct) =>
                {
                    var personaltitle = "";
                    var uidperson = "";
                    var exitdate = "";

                    foreach (var column in posted.columns)
                    {
                        if (column.column == "uidperson")
                        {
                            uidperson = column.value;
                        }
                    }

                    var query1 = Query.From("Person").Select("*").Where(String.Format("UID_Person = '{0}'", uidperson));
                    var tryget = await qr.Session.Source().TryGetAsync(query1, EntityLoadType.DelayedLogic, ct).ConfigureAwait(false);

                    if (tryget.Success)
                    {
                        foreach (var column in posted.columns)
                        {
                            if (column.column == "Job Description")
                            {
                                personaltitle = column.value;
                                await tryget.Result.PutValueAsync("PersonalTitle", personaltitle, ct).ConfigureAwait(false);
                            }
                            if (column.column == "Exit Date")
                            {
                                exitdate = column.value;
                                await tryget.Result.PutValueAsync("ExitDate", exitdate, ct).ConfigureAwait(false);
                            }
                        }

                        using (var u = qr.Session.StartUnitOfWork())
                        {
                            await u.PutAsync(tryget.Result, ct).ConfigureAwait(false);
                            await u.CommitAsync(ct).ConfigureAwait(false);
                        }
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
