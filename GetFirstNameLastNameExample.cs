// Import necessary namespaces
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
using System.Runtime.ConstrainedExecution;
using System.Security.Principal;

namespace QBM.CompositionApi
{
    // The GetFirstNameLastNameExample class implements the IApiProvider interfaces for the PortalApiProject
    public class GetFirstNameLastNameExample : IApiProviderFor<QER.CompositionApi.Portal.PortalApiProject>, IApiProvider
    {
        // The Build method is used to define API methods using the IApiBuilder
        public void Build(IApiBuilder builder)
        {
            // Add a GET method named "example/nameofloggedinuser" to the API
            builder.AddMethod(Method.Define("example/nameofloggedinuser")
                .HandleGet(async (qr, ct) =>
                {
                    // Retrieve the UID of the currently logged-in user from the session
                    var loggedinuser = qr.Session.User().Uid;

                    // Build a query to select FirstName and LastName from the "Person" table
                    // where the UID_Person matches the logged-in user's UID
                    var query = Query.From("Person")
                        .Select("FirstName", "LastName")
                        .Where(string.Format(@"UID_Person IN (
                            SELECT UID_Person FROM Person WHERE UID_Person = '{0}'
                        )", loggedinuser));

                    // Attempt to retrieve the entity matching the query asynchronously
                    var tryGet = await qr.Session.Source()
                        .TryGetAsync(query, EntityLoadType.DelayedLogic)
                        .ConfigureAwait(false);

                    // Convert the retrieved entity to a ReturnedName object and return it
                    return await ReturnedName.fromEntity(tryGet.Result, qr.Session)
                        .ConfigureAwait(false);
                }));
        }

        // The ReturnedName class represents the structure of the data returned to the client
        public class ReturnedName
        {
            // Properties to hold the first name and last name of the user
            public string FirstName { get; set; }
            public string LastName { get; set; }

            // Static method to create a ReturnedName instance from an IEntity object
            public static async Task<ReturnedName> fromEntity(IEntity entity, ISession session)
            {
                // Instantiate a new ReturnedName object and populate it with data from the entity
                var g = new ReturnedName
                {
                    // Asynchronously get the FirstName value from the entity
                    FirstName = await entity.GetValueAsync<string>("FirstName").ConfigureAwait(false),

                    // Asynchronously get the LastName value from the entity
                    LastName = await entity.GetValueAsync<string>("LastName").ConfigureAwait(false),
                };

                // Return the populated ReturnedName object
                return g;
            }
        }
    }
}
