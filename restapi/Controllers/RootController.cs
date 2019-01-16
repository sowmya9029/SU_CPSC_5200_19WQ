using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using restapi.Models;

namespace restapi.Controllers
{
    public class RootController : Controller
    {
        // GET api/values
        [Route("~/")]
        [HttpGet]
        [Produces(ContentTypes.Root)]
        [ProducesResponseType(typeof(IDictionary<ApplicationRelationship, DocumentLink>), 200)]
        public IDictionary<ApplicationRelationship, IList<DocumentLink>> Get()
        {
            Dictionary<ApplicationRelationship, IList<DocumentLink>> dictionary = new Dictionary<ApplicationRelationship, IList<DocumentLink>>();
            IList<DocumentLink> links = new List<DocumentLink>() {
                new DocumentLink() 
                    { 
                        Method = Method.Get,
                        Type = ContentTypes.Timesheets,
                        Relationship = DocumentRelationship.Timesheets,
                        Reference = "/timesheets"
                    },
                    new DocumentLink() 
                    { 
                        Method = Method.Post,
                        Type = ContentTypes.Timesheets,
                        Relationship = DocumentRelationship.CreateTimesheets,
                        Reference = "/timesheets"
                    }
            };
            dictionary.Add(ApplicationRelationship.Timesheets,links);
            return dictionary;
        }
    }
}
