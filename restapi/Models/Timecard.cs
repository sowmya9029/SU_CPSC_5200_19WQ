using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace restapi.Models
{
    public class Timecard
    {
       private float lineNumber=0;
        public Timecard(int resource)
        {
            Resource = resource;
            UniqueIdentifier = Guid.NewGuid();
            
            Identity = new TimecardIdentity();
            Lines = new List<AnnotatedTimecardLine>();
            Transitions = new List<Transition> { 
                new Transition(new Entered() { Resource = resource }) 
            };
        }

        public int Resource { get; private set; }
        
        [JsonProperty("id")]
        public TimecardIdentity Identity { get; private set; }

        public TimecardStatus Status { 
            get 
            { 
                return Transitions
                    .OrderByDescending(t => t.OccurredAt)
                    .First()
                    .TransitionedTo;
            } 
        }

        public DateTime Opened;

        [JsonProperty("recId")]
        public int RecordIdentity { get; set; } = 0;

        [JsonProperty("recVersion")]
        public int RecordVersion { get; set; } = 0;

        public Guid UniqueIdentifier { get; set; }

        [JsonIgnore]
        public IList<AnnotatedTimecardLine> Lines { get; set; }

        [JsonIgnore]
        public IList<Transition> Transitions { get; set; }

        public IList<ActionLink> Actions { get => GetActionLinks(); }
    
        [JsonProperty("documentation")]
        public IList<DocumentLink> Documents { get => GetDocumentLinks(); }

        public string Version { get; set; } = "timecard-0.2";

        private IList<ActionLink> GetActionLinks()
        {
            var links = new List<ActionLink>();

            switch (Status)
            {
                case TimecardStatus.Draft:
                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.Cancellation,
                        Relationship = ActionRelationship.Cancel,
                        Reference = $"/timesheets/{Identity.Value}/cancellation"
                    });

                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.Submittal,
                        Relationship = ActionRelationship.Submit,
                        Reference = $"/timesheets/{Identity.Value}/submittal"
                    });

                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.TimesheetLine,
                        Relationship = ActionRelationship.RecordLine,
                        Reference = $"/timesheets/{Identity.Value}/lines"
                    });

                    links.Add(new ActionLink() {
                        Method = Method.Delete,
                        Type = ContentTypes.TimesheetLine,
                        Relationship = ActionRelationship.Remove,
                        Reference = $"/timesheets/{Identity.Value}/remove"
                    });
                
                    break;

                case TimecardStatus.Submitted:
                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.Cancellation,
                        Relationship = ActionRelationship.Cancel,
                        Reference = $"/timesheets/{Identity.Value}/cancellation"
                    });

                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.Rejection,
                        Relationship = ActionRelationship.Reject,
                        Reference = $"/timesheets/{Identity.Value}/rejection"
                    });

                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.Approval,
                        Relationship = ActionRelationship.Approve,
                        Reference = $"/timesheets/{Identity.Value}/approval"
                    });

                    
                    links.Add(new ActionLink() {
                        Method = Method.Delete,
                        Type = ContentTypes.TimesheetLine,
                        Relationship = ActionRelationship.Remove,
                        Reference = $"/timesheets/{Identity.Value}/remove"
                    });
              

                    links.Add(new ActionLink() {
                        Method = Method.Post,
                        Type = ContentTypes.TimesheetLine,
                        Relationship = ActionRelationship.Replace,
                        Reference = $"/timesheets/{Identity.Value}/replace"
                    });
                    

                      links.Add(new ActionLink() {
                        Method = Method.Patch,
                        Type = ContentTypes.TimesheetLine,
                        Relationship = ActionRelationship.Update,
                        Reference = $"/timesheets/{Identity.Value}/update"
                    });
                    break;

                case TimecardStatus.Approved:
                    // terminal state, nothing possible here
                    break;

                case TimecardStatus.Cancelled:
                    // terminal state, nothing possible here
                    break;

                case TimecardStatus.Removed:
                    // terminal state, nothing possible here
                    break;

                  
            }

            return links;
        }

        private IList<DocumentLink> GetDocumentLinks()
        {
            var links = new List<DocumentLink>();

            links.Add(new DocumentLink() {
                Method = Method.Get,
                Type = ContentTypes.Transitions,
                Relationship = DocumentRelationship.Transitions,
                Reference = $"/timesheets/{Identity.Value}/transitions"
            });

            if (this.Lines.Count > 0)
            {
                links.Add(new DocumentLink() {
                    Method = Method.Get,
                    Type = ContentTypes.TimesheetLine,
                    Relationship = DocumentRelationship.Lines,
                    Reference = $"/timesheets/{Identity.Value}/lines"
                });
            }

            if (this.Status == TimecardStatus.Submitted)
            {
                links.Add(new DocumentLink() {
                    Method = Method.Get,
                    Type = ContentTypes.Transitions,
                    Relationship = DocumentRelationship.Submittal,
                    Reference = $"/timesheets/{Identity.Value}/submittal"
                });
            }

            return links;
        }

        public AnnotatedTimecardLine AddLine(TimecardLine timecardLine,Timecard timecard)
        {
            Console.WriteLine("length----"+timecard.Lines.Count);
            
          
          /*if(timecard.Lines.Count>0)
            {
                 lineNumber = timecard.Lines[timecard.Lines.Count-1].LineNumber;
            }
            else
            {
                 lineNumber = 0;
            }*/
           
            var annotatedLine = new AnnotatedTimecardLine(timecardLine,++lineNumber);
            Lines.Add(annotatedLine);
            return annotatedLine;
        }


        public AnnotatedTimecardLine ReplaceLine(Timecard timecard,TimecardPutLine timecardLine)
        {
           
            
             
                for(int i =0;i<timecard.Lines.Count;i++)
                {  
                 AnnotatedTimecardLine l = timecard.Lines[i];
                 //replaces the whole dictionary line item when the linenumber matches default values are 0
                 if((l.LineNumber == timecardLine.LineNumber))
                    {
                      l.Week = timecardLine.Week;
                      l.Day = timecardLine.Day;
                      l.Year = timecardLine.Year;
                      l.Hours = timecardLine.Hours;
                      l.Project = timecardLine.Project; 
                      return l;
                    }
                }
                              
            
            
            return null;
        }

        public AnnotatedTimecardLine UpdateLine(Timecard timecard,TimecardPatchLine timecardLine)
        {
         
                for(int i =0;i<timecard.Lines.Count;i++)
                { 
                    AnnotatedTimecardLine l = timecard.Lines[i];
                    try
                    {     
                        //if the line number matches checks if the given values are not null and updates the dictionary with provided data               
                        if((l.LineNumber == timecardLine.LineNumber))
                            {
                            
                            
                            if(timecardLine.Hours != null)
                                l.Hours = timecardLine.Hours.GetValueOrDefault();

                            if(timecardLine.Project != null)
                                l.Project = timecardLine.Project;
                            return l;
                            }
                       
                    } 
                    catch(FormatException)
                    {
                        Console.WriteLine("format exception");
                        return null;
                    }
                    
                }
          return null;
        }
                         
            
        

       
    }
}