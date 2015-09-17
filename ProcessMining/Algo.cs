using ProcessMining.DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessMining.Helpers;
using System.Data;

namespace ProcessMining
{
    public class Algo
    {
        private List<Log> _logs;
        private ExcelDataProvider _provider;

        public List<List<Log>> AllProcess;

        public List<Log> Logs
        {
            get
            {
                if (_logs == null)
                {
                    var allLogs = _provider.LogList;
                    _logs = allLogs;
                }
                return _logs;
            }
        }
        public List<Event> T_W { get; set; }
        public List<Event> T_I { get; set; }
        public List<Event> T_O { get; set; }
        public List<EventPair> X_W { get; set; }
        public List<EventPair> Y_W { get; set; }
        public IEnumerable<ProcessOccurence> Occurences { get; set; }
        public char[,] Matrix;

        public Algo(string ExcelFilePath)
        {
            _provider = new Helpers.ExcelDataProvider();
            _provider.Init(ExcelFilePath);
        }
        public void Process()
        {
            //Identify completed process instances
            var instances = Logs
                                .OrderBy(t => t.StartTime)
                                .GroupBy(t => t.InstanceId)
                                .Where(t => t.Last().EventId == -1)
                                .ToList();

            //Identify occurences per process
            Occurences = from process in instances
                         group process by process.First().Process into grp
                         select new ProcessOccurence
                         {
                             Process = grp.Key.Id,
                             Occurence = grp.Count()
                         };

            AllProcess = new List<List<Log>>();
            foreach (var process in instances)
            {
                AllProcess.Add(process.Where(t => t.EventId != -1).ToList());
            }

            GC.Collect();

            //Identify all events which occurred at least once
            T_W = Logs
                        .Where(t => t.EventId != -1)
                        .Select(t => t.Event)
                        .Distinct().ToList();

            GC.Collect();

            //Identify the start events
            T_I = (from log in AllProcess
                   select log.First().Event)
                      .GroupBy(t => t.Id)
                      .Select(t => t.First())
                      .ToList();

            GC.Collect();

            //Identify the end events
            T_O = (from log in AllProcess
                   select log.Where(t => t.EventId != -1).Last().Event)
                      .GroupBy(t => t.Id)
                      .Select(t => t.First())
                      .ToList();
            //Generate Matrix
            Matrix = GenerateMatrix();

            GC.Collect();

            //Get HASHes from matrix
            var Hashes = GetHashes(Matrix);

            GC.Collect();

            //Get Forwards from matrix
            var Forwards = GetForwards(Matrix);

            GC.Collect();

            //Get X_W
            X_W = GetNormalizedPairs(Hashes: Hashes, Forwards: Forwards);

            GC.Collect();

            //Get Y_W
            Y_W = RemoveRedundancy(X_W);

            GC.Collect();


        }

        private char[,] GenerateMatrix()
        {
            var eventsCount = T_W.Count;


            //Creating blank matrix
            var matrix = new char[eventsCount, eventsCount];

            //Traversing events wrt matrix : START
            for (int i = 0; i < eventsCount; i++)
            {
                for (int j = 0; j < eventsCount; j++)
                {
                    //Cancelling diagonal fields
                    if (i == j)
                    {
                        matrix[i, j] = '-';
                        continue;
                    }
                    char occurenceType = '#';
                    var firstEvent = T_W.ElementAt(i);
                    var secondEvent = T_W.ElementAt(j);
                    bool traversalComplete = false;
                    foreach (var process in AllProcess)
                    {
                        var procList = process.OrderBy(t => t.StartTime).ToList();
                        for (int k = 0; k < procList.Count; k++)
                        {
                            var element = procList.ElementAt(k);
                            var postElement = k < procList.Count - 1 ? procList.ElementAt(k + 1) : null;
                            if (postElement != null && element.EventId == firstEvent.Id && postElement.EventId == secondEvent.Id)
                            {
                                if (occurenceType == '<')
                                {
                                    occurenceType = '|';
                                    traversalComplete = true;
                                }
                                else occurenceType = '>';
                            }
                            if (postElement != null && postElement.EventId == firstEvent.Id && element.EventId == secondEvent.Id)
                            {
                                if (occurenceType == '>')
                                {
                                    occurenceType = '|';
                                    traversalComplete = true;
                                }
                                else occurenceType = '<';
                            }
                            if (traversalComplete)
                            {
                                traversalComplete = false;
                                break;
                            }
                        }
                    }
                    matrix[i, j] = occurenceType;
                    occurenceType = '#';
                }
            }
            //Traversing events wrt matrix : END
            return matrix;
        }
        private List<EventPair> GetHashes(char[,] Matrix)
        {
            var Hashes = new List<EventPair>();
            for (int i = 0; i < T_W.Count(); i++)
            {
                for (int j = 0; j < T_W.Count(); j++)
                {
                    if (Matrix[i, j] == '#') Hashes.Add(new EventPair { Left = new List<Event> { T_W.ElementAt(i) }, Right = new List<Event> { T_W.ElementAt(j) } });
                }
            }
            return Hashes;
        }
        private List<EventPair> GetNormalizedPairs(List<EventPair> Hashes, List<EventPair> Forwards)
        {
            var x_w = Forwards.ToList();
            var X_Temp = Forwards.ToList();
            //Check if Hashes come with others
            foreach (var hash in Hashes)
            {
                var eventsToMatch = T_W.Where(t => t.Id != hash.Left.First().Id && t.Id != hash.Right.First().Id).ToList();
                foreach (var matchEvent in eventsToMatch)
                {
                    var pairToMatch1 = new EventPair { Left = new List<Event> { matchEvent }, Right = new List<Event> { hash.Left.First() } };
                    var pairToMatch2 = new EventPair { Left = new List<Event> { matchEvent }, Right = new List<Event> { hash.Right.First() } };
                    if (X_Temp.ContainsEventPair(pairToMatch1) && X_Temp.ContainsEventPair(pairToMatch2))
                    {
                        var eventPair = new EventPair { Left = new List<Event> { matchEvent }, Right = new List<Event> { pairToMatch1.Right.First(), pairToMatch2.Right.First() } };
                        if (!x_w.ContainsEventPair(eventPair))
                            x_w.Add(eventPair);
                    }

                    pairToMatch1 = new EventPair { Right = new List<Event> { matchEvent }, Left = new List<Event> { hash.Left.First() } };
                    pairToMatch2 = new EventPair { Right = new List<Event> { matchEvent }, Left = new List<Event> { hash.Right.First() } };
                    if (X_Temp.ContainsEventPair(pairToMatch1) && X_Temp.ContainsEventPair(pairToMatch2))
                    {
                        var eventPair = new EventPair { Left = new List<Event> { pairToMatch1.Left.First(), pairToMatch2.Left.First() }, Right = new List<Event> { matchEvent } };
                        if (!x_w.ContainsEventPair(eventPair))
                            x_w.Add(eventPair);
                    }
                }
            }
            return x_w;


        }
        private List<EventPair> GetForwards(char[,] Matrix)
        {
            var x_w = new List<EventPair>();
            for (int i = 0; i < T_W.Count(); i++)
            {
                for (int j = 0; j < T_W.Count(); j++)
                {
                    if (Matrix[i, j] == '>') x_w.Add(new EventPair { Left = new List<Event> { T_W.ElementAt(i) }, Right = new List<Event> { T_W.ElementAt(j) } });
                }
            }

            return x_w;
        }
        private List<EventPair> RemoveRedundancy(List<EventPair> X_W)
        {
            //Remove redundant event pairs
            var X_Temp = new List<EventPair>();

            foreach (var toMatch in X_W)
            {
                foreach (var pair in X_W)
                {
                    if (X_W.IndexOf(pair) == X_W.IndexOf(toMatch)) continue;
                    if (pair.Left.Intersect(toMatch.Left).Count() == toMatch.Left.Count() && pair.Right.Intersect(toMatch.Right).Count() == toMatch.Right.Count())
                    {
                        X_Temp.Add(toMatch);
                    }
                }
            }
            //Prepare Y_W by removing redundancy from X_W
            var y_w = X_W.ToList();
            foreach (var item in X_Temp)
            {
                y_w.Remove(item);
            }
            return y_w;
        }
    }

    public static class Helper
    {
        public static bool ContainsEventPair(this List<EventPair> collection, EventPair toMatch)
        {
            if (toMatch.Left.Count == 1 && toMatch.Right.Count == 1)
            {
                foreach (var pair in collection)
                {
                    if (pair.Left.Contains(toMatch.Left.First()) && pair.Right.Contains(toMatch.Right.First()))
                        return true;
                }
                return false;
            }
            else
            {
                foreach (var pair in collection)
                {
                    if (pair.Left.Intersect(toMatch.Left).Count() == toMatch.Left.Count() && pair.Right.Intersect(toMatch.Right).Count() == toMatch.Right.Count())
                        return true;
                }
                return false;
            }
        }
        public static string GetText(this List<Event> events)
        {
            var retVal = " { ";
            foreach (var obj in events)
            {
                retVal += obj.EventName + (events.IndexOf(obj) == events.Count() - 1 ? "" : ", ");
            }
            retVal += " } ";
            return retVal;
        }
        public static string GetText(this List<EventPair> pairs)
        {
            var retVal = " { ";
            foreach (var obj in pairs)
            {
                retVal += "({";
                foreach (var events in obj.Left)
                {
                    retVal += events.EventName + (obj.Left.IndexOf(events) == obj.Left.Count() - 1 ? "" : ", ");
                }
                retVal += "},{";
                foreach (var events in obj.Right)
                {
                    retVal += events.EventName + (obj.Right.IndexOf(events) == obj.Right.Count() - 1 ? "" : ", ");
                }
                retVal += "})" + (pairs.IndexOf(obj) == pairs.Count() - 1 ? "" : ", ");
            }
            retVal += " } ";
            return retVal;
        }
        public static string GetText(this List<List<Log>> processes, List<ProcessOccurence> Occurences)
        {
            var retVal = " { ";
            List<int> AlreadyOccured = new List<int>();
            foreach (var obj in processes)
            {
                if (AlreadyOccured.Contains(obj.First().ProcessId)) continue;
                retVal += "< ";
                foreach (var item in obj)
                {
                    retVal += item.Event.EventName;
                }
                retVal += " >";
                var occured = Occurences.Where(t => t.Process == obj.First().ProcessId).First().Occurence;
                retVal += occured > 1 ? "^" + occured + " " : "";
                retVal += (processes.IndexOf(obj) == processes.Count() - 1 ? " " : ", ");
                AlreadyOccured.Add(obj.First().ProcessId);
            }
            retVal = retVal.Trim().TrimEnd(',') + " } ";
            return retVal;
        }
        public static string GetP_W(this Algo algo)
        {
            var pairs = algo.Y_W;
            var retVal = " { i_W, o_W, ";

            foreach (var obj in pairs)
            {
                retVal += "P({";
                foreach (var events in obj.Left)
                {
                    retVal += events.EventName + (obj.Left.IndexOf(events) == obj.Left.Count() - 1 ? "" : ", ");
                }
                retVal += "},{";
                foreach (var events in obj.Right)
                {
                    retVal += events.EventName + (obj.Right.IndexOf(events) == obj.Right.Count() - 1 ? "" : ", ");
                }
                retVal += "})" + (pairs.IndexOf(obj) == pairs.Count() - 1 ? "" : ", ");
            }
            retVal += " } ";
            return retVal;
        }
        public static string GetF_W(this Algo algo)
        {
            var pairs = algo.Y_W;
            var retVal = " { ";
            foreach (var obj in algo.T_I)
            {
                retVal += "(i_W , " + obj.EventName + " ), ";
            }
            foreach (var obj in pairs)
            {

                var pathText = " P({";
                foreach (var events in obj.Left)
                {
                    pathText += events.EventName + (obj.Left.IndexOf(events) == obj.Left.Count() - 1 ? "" : ", ");
                }
                pathText += "},{";
                foreach (var events in obj.Right)
                {
                    pathText += events.EventName + (obj.Right.IndexOf(events) == obj.Right.Count() - 1 ? "" : ", ");
                }
                pathText += "})";

                foreach (var events in obj.Left)
                {
                    retVal += "("+events.EventName + "," + pathText + "), "; 
                }
                foreach (var events in obj.Right)
                {
                    retVal += "(" + pathText + ", " + events.EventName + "), ";
                }

            }
            foreach (var obj in algo.T_O)
            {
                retVal += "( " + obj.EventName + " , o_W ), ";
            }
            retVal = retVal.Trim().TrimEnd(',') + " } ";
            return retVal;
        }
        public static List<EventGridItem> GetEventsReport(this Algo algo)
        {
            var retVal = new List<EventGridItem>();
            foreach (var t in algo.T_W)
            {
                var obj = new EventGridItem { Activity = t.EventName, Originators = string.Empty };
                var occurences = algo.Logs.Where(s => s.EventId == t.Id);
                int totalTime = 0;
                int minTime = int.MaxValue;
                int maxTime = 0;
                foreach (var occurence in occurences)
                {
                    obj.Originators += obj.Originators.Contains(occurence.Employee.Name) ? "" : occurence.Employee.Name + ", ";
                    var time = (occurence.EndTime - occurence.StartTime).Milliseconds;
                    totalTime += time;
                    minTime = minTime > time ? time : minTime;
                    maxTime = maxTime < time ? time : maxTime;
                }
                obj.Originators = obj.Originators.Trim().TrimEnd(',');
                obj.MaxTime = maxTime;
                obj.MinTime = minTime;
                obj.AvgTime = totalTime / occurences.Count();
                retVal.Add(obj);
            }
            return retVal;
        }
        public static List<TransitionGridItem> GetTransitionReport(this Algo algo)
        {
            return null;
        }
    }
}
