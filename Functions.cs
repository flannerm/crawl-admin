using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crawl.DataAccess;
using Oracle.DataAccess.Client;
using System.Data;
using System.Configuration;
using System.Diagnostics;

namespace Crawl
{
    public class Functions
    {
        public int giCrawlNextOverallPick;
        public int giLastPickRound;
        public int giLastRoundCrawled;
        public int giPlayersPerString;
        public bool gbCrawlTeamPicks = false;
        public int giLastTeamCrawlString;
        public int giLastTeamCrawlPick;
        public int giLastCrawlString;

        public string gsDraftUpdateString;
        public bool gbAddToCrawl;
        public int giLastCrawledPickNum;
        public int giTextShowing;
        public int giLastCrawlUpdated;
        public RoundInfo[] gtRoundInfo = new RoundInfo[10];
        public int giTeamsPerString;
        public int giMaxRounds;
        public int giMessageCycleCounter;

        public double gdCrawlSpeed;
        public TeamDBSaver[] gtTeams = new TeamDBSaver[32];

        public bool noTeamCrawl = true;

        public int giCurrentRound = 0;
        
        public struct NextPick
        {
            public int iRound;
            public int iPick;
            public int iOverall;
            public int iTeam;
        }

        public struct RoundInfo
        {
            public int iRound;
            public int iLastPick;
            public int iFirstOverallPick;
            public int iLastOverallPick;
            public int iShowTeams;
            public int iShowRound;
        }

        public struct TeamDBSaver
        {
            public string sTeamAbbrev;
            public string sTeamCity;
            public string sTeamName;
            public string sFontColor;
        }     

        public void StopCrawl()
        {
            //NextPick tNextPick = FindNextTeamPick();

            //if (tNextPick.iRound != 2)
            //{
            //    giLastCrawledPickNum = 999;
            //}

            giLastCrawledPickNum = 0;

            giTextShowing = 1;

            giLastCrawlUpdated = 0;
        }

        public void InitializeCrawl()
        {
            NextPick tPick;
		
		    giLastCrawledPickNum = 0;
		    giPlayersPerString = 2;
		    giTeamsPerString = 6;
		    gdCrawlSpeed = 3;
		    giMaxRounds = Convert.ToInt16(ConfigurationManager.AppSettings["MaxRounds"].ToString());
		
		    giLastTeamCrawlString = 0;
		    giLastTeamCrawlPick = 1;
		
		    FindRounds();

            //Initialze showing the crawls.  Show all, but only show teams
            //  coming up for the first round.  Should change this to be a
            //  a little more intelligent soon.
		    
		    tPick = FindNextTeamPick();
		    if (tPick.iRound < 1)
            {
			    tPick.iRound = giMaxRounds;
		    }
            
            //SetRoundCrawlState(tPick.iRound);
        }

        public string AcquireString(int iPickStart, int iFlag)
        {
            int iPickEnd;
            int iRound;
            int iTeams;
            int iFirstRoundPick;
            int iMaxRoundPick;
            int iGetMessage;
            int iGetTeams;
            string sCrawl;
            int iSmallStr;
            int iSmallTP;
            NextPick tNextPick;

            if (gbCrawlTeamPicks == true)
            {
                sCrawl = GetTeamPickCrawlString(giLastCrawlString, giLastTeamCrawlPick);

                if (sCrawl != "")
                {
                    return sCrawl;
                }
            }

            iGetMessage = 0;
            iGetTeams = 0;

            tNextPick = FindNextTeamPick();

            iRound = FindPickRound(iPickStart);
            iFirstRoundPick = gtRoundInfo[iRound].iFirstOverallPick;
            iMaxRoundPick = gtRoundInfo[iRound].iLastOverallPick;

            sCrawl = "";

            if (iPickStart == iFirstRoundPick)
            {
                sCrawl += "  ROUND " + iRound.ToString().Trim() + ".    ";
            }

            iSmallStr = 10;
            iSmallTP = 0;

            if (iPickStart >= giCrawlNextOverallPick)
            {
                if (iFlag == 1)
                {
                    iPickEnd = iPickStart + 2;
                }
                else
                {
                    iPickEnd = iPickStart + giTeamsPerString - 1;
                }

                if (iPickEnd >= iMaxRoundPick)
                {
                    iPickEnd = iMaxRoundPick;
                    iSmallStr = iPickEnd - iPickStart + 1;
                    iSmallTP = 1;
                    iGetMessage = 1;
                }

                iTeams = 1;
            }
            else
            {
                //We are showing the picks!
                if (iFlag == 1)
                {
                    //Just one more guy!
                    iPickEnd = iPickStart;
                }
                else
                {
                    iPickEnd = iPickStart + giPlayersPerString - 1;
                }

                if (iPickEnd >= (giCrawlNextOverallPick - 1))
                {
                    //We can't show players if they haven't been picked yet
                    iPickEnd = giCrawlNextOverallPick - 1;

                    if (gtRoundInfo[iRound].iShowTeams == 0 || iPickEnd >= iMaxRoundPick)
                    {
                        //We're not allowed to show teams or we're at the end of this round
                        iPickEnd = iMaxRoundPick;
                        iGetMessage = 1;
                    }
                    else if (gtRoundInfo[iRound].iShowTeams == 1)
                    {
                        //Well, maybe we can show teams
                        iGetTeams = 1;
                    }
                }
                else if (iPickEnd >= iMaxRoundPick)
                {
                    //We've gone beyond the end of this round.  Stop there!
                    iPickEnd = iMaxRoundPick;
                    iSmallStr = iPickEnd - iPickStart + 1;
                    iSmallTP = 2;
                    iGetMessage = 1;
                }

                iTeams = 0;

            }

            if (iTeams == 0)
            {
                //Get the players picked
                sCrawl += GetPickCrawlString(iPickStart, iPickEnd);
            }
            else
            {
                //Get the teams to pick
                sCrawl += GetToPickCrawlString(iPickStart, iPickEnd);
            }
           
            if (iGetMessage == 1 && Properties.Settings.Default.ShowMessage == "1" && Properties.Settings.Default.Message.ToString().Trim() != "")
            {
                if (iPickEnd == iMaxRoundPick && iRound < 10 && gtRoundInfo[iRound + 1].iShowTeams == 1)
                {
                    //DO NOTHING - We have more stuff to show before we start over!
                    iSmallTP = 1;
                    iSmallStr = 2;
                }
                else
                {
                    //We can append the message as well
                    sCrawl += Properties.Settings.Default.Message.ToString().Trim() + "       "; //GetCycleMessages();

                    if (tNextPick.iRound >= 3 && noTeamCrawl == false)
                    {
                        gbCrawlTeamPicks = true;

                        if (giLastTeamCrawlString >= Convert.ToInt16(ConfigurationManager.AppSettings["TeamCount"].ToString()))
                        {
                            giLastTeamCrawlString = 1;
                        }
                        else
                        {
                            giLastTeamCrawlString += 1;
                        }

                        giLastTeamCrawlPick = 1;
                    }
                }
            }
            else if (iGetTeams == 1 && iFlag == 0)
            {
                //We didn't fill the string, let's put in some teams
                iPickStart = iPickEnd + 1;
                iPickEnd = iPickStart + 1;

                if (iPickEnd > iMaxRoundPick)
                {
                    //But we can't go past the end
                    iPickEnd = iMaxRoundPick;
                }

                if (iPickEnd == iMaxRoundPick)
                {
                    iGetMessage = 1;
                }

                //Add whatever teams we can
                sCrawl += GetToPickCrawlString(iPickStart, iPickEnd);

                if (iGetMessage == 1 && Properties.Settings.Default.ShowMessage == "1" && Properties.Settings.Default.Message.ToString().Trim() != "")
                {
                    if (iPickEnd == iMaxRoundPick && iRound < 10 && gtRoundInfo[iRound + 1].iShowTeams == 1)
                    {
                        //DO NOTHING - We have more stuff to show before we start over!
                        iSmallTP = 1;
                        iSmallStr = 2;
                    }
                    else
                    {
                        //If we got to the end, append messages as well!
                        sCrawl += Properties.Settings.Default.Message.ToString().Trim() + "       "; //GetCycleMessages();

                        if (tNextPick.iRound >= 3 && noTeamCrawl == false)
                        {
                            gbCrawlTeamPicks = true;

                            if (giLastTeamCrawlString >= 32)
                            {
                                giLastTeamCrawlString = 1;
                            }
                            else
                            {
                                giLastTeamCrawlString += 1;
                            }

                            giLastTeamCrawlPick = 1;
                        }
                    }
                }
            }

            //Remember the last thing we did
            giLastCrawledPickNum = iPickEnd;

            //Make sure no bad characters in there!
            sCrawl = sCrawl.Replace("`", "'");

            if (iFlag == 1)
            {
                //So we don't keep coming in here!
                iSmallTP = 0;
            }

            if ((iSmallTP == 1 && iSmallStr < 3) || (iSmallTP == 2 && iSmallStr < 2))
            {
                iPickEnd = FindNextPickStart();
                sCrawl += AcquireString(iPickEnd, 1);
            }

            return sCrawl;
        }

        public int FindNextPickStart()
        {
            int iNewPS;
            int iGoodNum = 0;
            int iRound = 0;
            //int iLoop;
            int iSafety = 10;

            //Start out with the next number and see if that's OK
            iNewPS = giLastCrawledPickNum + 1;

            while (iGoodNum != 1 && iSafety > 0)
            {
                //Find what round we're trying to be in
                iRound = FindPickRound(iNewPS);

                if (iRound == 0)
                {
                    //We've gone too far!  Start over!
                    iNewPS = 1;
                    iRound = 1;
                }

                if (gtRoundInfo[iRound].iShowRound == 0)
                {
                    //We don't want to show this round!  Try the first pick in the next round!
                    if (iRound == giMaxRounds)
                    {
                        iRound = 1;
                    }
                    else
                    {
                        iRound += 1;
                    }

                    iNewPS = gtRoundInfo[iRound].iFirstOverallPick;
                }
                else
                {
                    //We can show this round
                    if (iNewPS >= giCrawlNextOverallPick)
                    {
                        //We've shown all picks.  Check if we can show teams to come.
                        if (gtRoundInfo[iRound].iShowTeams == 0)
                        {
                            //We can't show teams!  Go to first pick of the next round
                            if (iRound == giMaxRounds)
                            {
                                iRound = 1;
                            }
                            else
                            {
                                iRound += 1;
                            }

                            iNewPS = gtRoundInfo[iRound].iFirstOverallPick;
                        }
                        else
                        {
                            //This is a good number to start with
                            iGoodNum = 1;
                        }
                    }
                    else
                    {
                        //This is a good number to start with
                        iGoodNum = 1;
                    }
                }

                iSafety -= 1;
            } //end while

            if (iSafety <= 0)
            {
                //We've tried 10 times and there should only be 7 rounds.  We're in trouble.
                //MsgBox("Damn!  Can't FindNextPickStart!")
            }

            Debug.Print("ipickstart " + iNewPS);

            return iNewPS;
        }

        public int FindPickRound(int iPick)
        {
            int iRound = 0;
            int iLoop;

            //Figure out which round the pick is in

            for (iLoop = 1; iLoop <= giMaxRounds; iLoop++)
            {
                if (iPick >= gtRoundInfo[iLoop].iFirstOverallPick && iPick <= gtRoundInfo[iLoop].iLastOverallPick)
                {
                    iRound = iLoop;
                    iLoop = giMaxRounds + 1;
                }
            }

            return iRound;
        }
        
        public NextPick FindNextTeamPick()
        {
            string sQuery = "select * from draftorder do join news_teams nt on do.teamid = nt.team_id where do.pick = (select max(pick) + 1 from draftplayers)";
            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataReader rdr = null;
            DataTable tbl;
            NextPick tFound = new NextPick();

            try
            {
                //Find the first pick int the table that has no player associated with it
                cn = DbConnection.createConnectionSDR();
                cmd = new OracleCommand(sQuery, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();

                tbl.Load(rdr);
                rdr.Close();
                rdr.Dispose();

                if (tbl.Rows.Count > 0)
                {
                    tFound.iRound = Convert.ToInt16(tbl.Rows[0]["round"]);
                    tFound.iPick = Convert.ToInt16(tbl.Rows[0]["roundpick"]);
                    tFound.iOverall = Convert.ToInt16(tbl.Rows[0]["pick"]);
                    tFound.iTeam = Convert.ToInt32(tbl.Rows[0]["teamid"]);

                    giCrawlNextOverallPick = tFound.iOverall;
                }
                else
                {
                    tFound.iRound = 1;
                    tFound.iPick = 1;
                    tFound.iOverall = 1;
                    //may need to find the team here
                }

                if (tFound.iPick == 1)
                {
                    giLastPickRound = tFound.iRound - 1;
                }
                else
                {
                    giLastPickRound = tFound.iRound;
                }

            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (rdr != null) rdr.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return tFound;
        }

        public string GetToPickCrawlString(int iPickFrom, int iPickTo)
        {
            Debug.Print("from: " + iPickFrom + ", to: " + iPickTo);

            string sCrawl = "";
            string sQuery;
            int iNewTeam;

            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataReader rdr = null;
            DataTable tbl;

            try
            {
                cn = DbConnection.createConnectionSDR();

                sQuery = "select * from draftorder a join news_teams b on a.teamid = b.team_id where (pick ";

                if (iPickTo == 0)
                {
                    //We just want one team, in iPickFrom
                    sQuery += "= " + iPickFrom.ToString().Trim() + ")";
                }
                else
                {
                    //We want all teams between iPickFrom to iPickTo
                    if (iPickFrom > iPickTo)
                    {
                        iPickFrom = iPickTo;
                    }

                    sQuery += " between " + iPickFrom.ToString().Trim();
                    sQuery += " and " + iPickTo.ToString().Trim();
                }

                sQuery += ") order by pick asc";

                //Go through the build the string with the teams
                cmd = new OracleCommand(sQuery, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();

                tbl.Load(rdr);
                rdr.Close();
                rdr.Dispose();

                foreach (DataRow row in tbl.Rows)
                {
                    iNewTeam = Convert.ToInt32(row["teamid"]);
                    sCrawl += row["pick"].ToString() + ". ";

                    switch (row["team_name"].ToString().ToUpper())
                    {
                        case "LAKERS":
                            sCrawl += "LA LAKERS        ";
                            break;
                        case "CLIPPERS":
                            sCrawl += "LA CLIPPERS        ";
                            break;
                        case "GIANTS":
                            sCrawl += "NY GIANTS        ";
                            break;
                        case "JETS":
                            sCrawl += "NY JETS        ";
                            break;
                        //case "49ERS":
                        //    sCrawl += "49ers        ";
                        //    break;
                        default:
                            sCrawl += row["city_st_name"].ToString().ToUpper() + "        ";
                            break;
                    }        
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (rdr != null) rdr.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            //Fire it back!
            return sCrawl;
        }

        public string GetPickCrawlString(int iPickFrom, int iPickTo)
        {
            string sCrawl = "";
            string sQuery;
            int iNewTeam;
            int iRound = 1;
            int iLastRound = -1;

            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataReader rdr = null;
            DataTable tbl;

            try
            {
                cn = DbConnection.createConnectionSDR();

                sQuery = "select a.*, b.*, c.*, d.*, (select text from drafttidbits where referencetype = 1 and referenceid = a.playerid and tidbitorder = 999) as tradetidbit ";
                sQuery += "from draftplayers a right join draftorder b on a.pick = b.pick ";
                sQuery += "join news_teams c on b.teamid = c.team_id join news_teams d on a.schoolid = d.team_id where (b.pick ";

                if (iPickTo == 0)
                {
                    //We just want one guy, in iPickFrom
                    sQuery += "= " + iPickFrom.ToString() + ")";
                }
                else
                {
                    //We want all guys between iPickFrom to iPickTo
                    if (iPickFrom > iPickTo)
                    {
                        iPickFrom = iPickTo;
                    }

                    sQuery += " between " + iPickFrom.ToString();
                    sQuery += " and " + iPickTo.ToString();
                }

                sQuery += ") order by b.pick asc";

                cn = DbConnection.createConnectionSDR();

                cmd = new OracleCommand(sQuery, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();

                tbl.Load(rdr);
                rdr.Close();
                rdr.Dispose();

                foreach (DataRow row in tbl.Rows)
                {
                    iNewTeam = Convert.ToInt32(row["teamid"]);
                    iRound = Convert.ToInt16(row["round"]);
                    iLastRound = iRound;

                    sCrawl += row["pick"].ToString() + ". ";

                    switch (row["team_name"].ToString().ToUpper())
                    {
                        case "LAKERS":
                            sCrawl += "LA LAKERS - ";
                            break;
                        case "CLIPPERS":
                            sCrawl += "LA CLIPPERS - ";
                            break;
                        case "GIANTS":
                            sCrawl += "NY GIANTS - ";
                            break;
                        case "JETS":
                            sCrawl += "NY JETS - ";
                            break;
                        //case "49ERS":
                        //    sCrawl += "49ers - ";
                        //    break;
                        default:
                            sCrawl += row["city_st_name"].ToString().ToUpper() + " - ";
                            break;
                    }                    
                    
                    sCrawl += "<font Klavika Regular><bold true>" + row["firstname"].ToString() + " " + row["lastname"].ToString() + " ";

                    if (row["position"].ToString().Trim() != "")
                    {
                        sCrawl += row["position"].ToString() + "/" + row["team_name1"].ToString();
                    }
                    else
                    {
                        sCrawl += row["d.team_name1"].ToString();
                    }

                    if (row["tradetidbit"].ToString().Trim() != "")
                    {
                        sCrawl += " " + row["tradetidbit"].ToString();
                    }                    

                    sCrawl += "<\\bold><\\font>        ";
                }

                giLastRoundCrawled = iRound;

            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (rdr != null) rdr.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

            return sCrawl;
        }

        public string GetTeamPickCrawlString(int iTeam, int iPickStart)
        {
            string sCrawl = "";
            string sQuery;
            //int iNewTeam;
            //int iRound = 1;
            //int iLastRound = -1;
            int iPlayers = giPlayersPerString;
            int iLoop = 0;

            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataReader rdr = null;
            DataTable tbl;

            try
            {
                sQuery = "select * from draftplayers a join news_teams b on a.schoolid = b.team_id right join draftorder c on a.pick = c.pick ";
                sQuery += "join news_teams d on c.teamid = d.team_id where c.teamid = " + iTeam + " order by c.pick asc";

                cn = DbConnection.createConnectionSDR();
                cmd = new OracleCommand(sQuery, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();

                tbl.Load(rdr);
                rdr.Close();
                rdr.Dispose();

                //DON'T KNOW WHAT'S GOING ON HERE/////////////////////////////////////////////////
                //Take each guy and put him in the string

                if (iPickStart <= 1 || tbl.Rows.Count > 0)
                {
                    sCrawl = "        " + tbl.Rows[0]["city_st_name"].ToString() + " PICKS -   ";
                }

                for (iLoop = 1; iLoop < (iPickStart - 1); iLoop++)
                {

                }
                //rstWork = gcnDraft.Execute(sQuery)
                //If (iPickStart <= 1) And (rstWork.EOF <> True) Then
                //    sCrawl = Space(8) & UCase(rstWork.Fields("TeamCity").Value) & " PICKS -   "
                //End If

                //For iLoop = 1 To (iPickStart - 1)
                //    If rstWork.EOF <> True Then
                //        rstWork.MoveNext()
                //    End If
                //Next iLoop
                ////////////////////////////////////////////////////////////////////////////

                foreach (DataRow row in tbl.Rows)
                {
                    if (iPlayers > 0)
                    {
                        sCrawl += "RD" + row["round"].ToString() + "/";
                        sCrawl += row["pick"].ToString() + ". ";
                        sCrawl += row["abbrev_4"].ToString() + " - ";
                        sCrawl += row["firstname"].ToString() + " " + row["lastname"].ToString() + " ";

                        if (row["position"].ToString() != "")
                        {
                            sCrawl += row["position"].ToString() + "/" + row["b.team_name"].ToString();
                        }
                        else
                        {
                            sCrawl += row["b.team_name"].ToString();
                        }

                        sCrawl += "        ";

                        iLoop++;

                        iPlayers -= 1;
                    }
                }

                giLastTeamCrawlString = iTeam;
                giLastTeamCrawlPick = iLoop;

                if (tbl.Rows.Count == 0 || iPlayers > 0)
                {
                    //we are out of players for team iTeam
                    if (iTeam % 8 == 0)
                    {
                        gbCrawlTeamPicks = false;
                    }
                    else
                    {
                        if (iPlayers > 0)
                        {
                            //it's not the last team and there may still be room in the string, try the next one
                            sCrawl += GetTeamPickCrawlString(iTeam + 1, 1);
                        }
                    }
                }

                return sCrawl;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }

        }

        public void FindRounds()
        {
            string sQuery;
            OracleConnection cn = null;
            OracleCommand cmd = null;
            OracleDataReader rdr = null;
            DataTable tbl;

            int iLastRound;
            int iRound;
            int iMaxPick = 0;
            int iOverall = 0;

            try
            {
                //initialize to 0
                for (iRound = 1; iRound <= giMaxRounds; iRound++)
                {
                    gtRoundInfo[iRound].iLastPick = 0;
                }

                //Round 1 is obvious
                iRound = 1;
                iLastRound = 1;
                gtRoundInfo[1].iFirstOverallPick = 1;

                cn = DbConnection.createConnectionSDR();

                //Go through the database and figure out what picks start
		        //and end each round
                sQuery = "select * from draftorder order by pick asc";
                cmd = new OracleCommand(sQuery, cn);
                rdr = cmd.ExecuteReader();

                tbl = new DataTable();
                tbl.Load(rdr);
                rdr.Close();
                rdr.Dispose();

                foreach (DataRow row in tbl.Rows)
                {
                    iRound = Convert.ToInt16(row["round"]);

                    if (iRound > 0)
                    {
                        iOverall = Convert.ToInt16(row["pick"]);

                        if (iRound != iLastRound)
                        {
                            //Save the info for that last round
                            gtRoundInfo[iLastRound].iRound = iLastRound;
                            gtRoundInfo[iLastRound].iLastPick = iMaxPick;
                            gtRoundInfo[iLastRound].iLastOverallPick = iOverall - 1;

                            //And start saving for this round
                            gtRoundInfo[iRound].iFirstOverallPick = iOverall;

                            iLastRound = iRound;
                        }
                        else
                        {
                            iMaxPick = Convert.ToInt16(row["roundpick"]);
                        }
                    }
                } //foreach

                giMaxRounds = iRound;

                gtRoundInfo[iRound].iRound = iRound;
                gtRoundInfo[iRound].iLastPick = iMaxPick;
                gtRoundInfo[iRound].iLastOverallPick = iOverall;

            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (cn != null) cn.Close(); cn.Dispose();
            }
        }
	
        public void SetRoundCrawlState(int iRound)
        {
 
            gtRoundInfo[iRound].iShowRound = 1;
            gtRoundInfo[iRound].iShowTeams = 1;

            //int iLoop;
            
            //switch (iRound)
            //{
            //    case 1:
            //        gtRoundInfo[1].iShowRound = 1;
            //        gtRoundInfo[1].iShowTeams = 1;

            //        for (iLoop = 2; iLoop < giMaxRounds; iLoop++)
            //        {
            //            gtRoundInfo[iLoop].iShowRound = 0;
            //            gtRoundInfo[iLoop].iShowTeams = 0;
            //        }

            //        break;
            //    case 2:
            //        //Only show for 1 and 2
            //        gtRoundInfo[1].iShowRound = 1;
            //        gtRoundInfo[1].iShowTeams = 1;
            //        gtRoundInfo[2].iShowRound = 1;
            //        gtRoundInfo[2].iShowTeams = 0;

            //        for (iLoop = 3; iLoop < giMaxRounds; iLoop++)
            //        {
            //            gtRoundInfo[iLoop].iShowRound = 0;
            //            gtRoundInfo[iLoop].iShowTeams = 0;
            //        }

            //        break;
            //    default: 
            //        //Show all rounds
            //        //for (iLoop = 1; iLoop < giMaxRounds; iLoop++)
            //        //{
            //        //    gtRoundInfo[iLoop].iShowRound = 1;
            //        //    gtRoundInfo[iLoop].iShowTeams = 1;
            //        //}

            //        gtRoundInfo[iRound].iShowRound = 1;
            //        gtRoundInfo[iRound].iShowTeams = 1;

            //        break;

            //}
        }

        public bool SetRoundCrawlState()
        {
            bool retVal = false;

            NextPick tNextPick = FindNextTeamPick();
                
            //if (tNextPick.iPick == 1)
            //{
            //    SetRoundCrawlState(tNextPick.iRound);
            //}
            if (tNextPick.iRound != giCurrentRound)
            {
                giCurrentRound = tNextPick.iRound;
                SetRoundCrawlState(giCurrentRound);

                retVal = true;
            }

            return retVal;
        }
    }
}
