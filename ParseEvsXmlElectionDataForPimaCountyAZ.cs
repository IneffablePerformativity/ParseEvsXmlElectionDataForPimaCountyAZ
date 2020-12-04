/*
 * ParseEvsXmlElectionDataForPimaCountyAZ.cs
 *
 * which code and results I will archive at:
 * https://github.com/IneffablePerformativity
 * https://github.com/IneffablePerformativity/ParseEvsXmlElectionDataForPimaCountyAZ
 * 
 * "To the extent possible under law, Ineffable Performativity has waived all copyright and related or neighboring rights to
 * The C# program ParseEvsXmlElectionDataForPimaCountyAZ.cs and resultant outputs.
 * This work is published from: United States."
 * 
 * This work is offered as per license: http://creativecommons.org/publicdomain/zero/1.0/
 * 
 * 
 * Goal: to demonstrate any inverse republicanism::trump relationship
 * as was described for Milwaukee County Wisconsin in an article at:
 * https://www.thegatewaypundit.com/2020/11/caught-part-3-impossible-ballot-ratio-found-milwaukee-results-change-wisconsin-election-30000-votes-switched-president-trump-biden/
 * 
 * to wit, visually:
 * https://www.thegatewaypundit.com/wp-content/uploads/2020-Milwaukee-Hockey-Stick-Chart.jpg
 * 
 * Or rather now, a systematic "BONUS TO BIDEN" and "THEFT FROM TRUMP"
 * by comparing RED and BLUE % for POTUS vs. SENUS and/or REPUS races.
 * 
 * 
 * Manual conclusion:
 * 
 * Very symmetrical Bonuses suggest no systematic fraud in Pima County Arizona.
 * 
 * 
 * Code conclusions:
 * 
 * 
 * 
 * Parsing the XML file downloaded from:
 * Googlng: pima county arizona election results
 * https://webcms.pima.gov/government/elections_department/
 * 2020 General Election XML ->
 * https://webcms.pima.gov/UserFiles/Servers/Server_6/File/Government/elections/Election%20Results/ENR%20STANDARD%20XML.txt
 * which File I saved as:
 * PIMA County AZ ENR STANDARD XML.txt
 * 
 * 
 * There is a series of similar programs building upon one another,
 * named similarly, with XML inputs, HTML inputs, PDF-as-TXT input,
 * and some ugly earlier versions, all to be found at GitHub, here:
 * https://github.com/IneffablePerformativity
 * 
 * 
 * also note, I manually compress final image at tinypng.com
 */


using System;
using System.Collections.Generic;
using System.Drawing; // Must add a reference to System.Drawing.dll
using System.Drawing.Imaging; // for ImageFormat
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;


namespace ParseEvsXmlElectionDataForPimaCountyAZ
{
	
	class Choice // AKA Candidate
	{
		public Choice(string party)
		{
			this.party = party;
		}
		public string party = "";
		public int votes = 0;
	}
	
	class Contest // AKA Race
	{
		public Dictionary<string,Choice> choices = new Dictionary<string,Choice>();
	}
	
	class Grain // AKA Precinct, County, Ward, etc.
	{
		public Grain(string locality)
		{
			this.locality = locality;
		}
		public string locality = "";
		public Dictionary<string,Contest> contests = new Dictionary<string,Contest>();
	}
	
	class ShrodingersCat // needed to make a reference to N sums, which one unknown yet
	{
		public int rep;
		public int dem;
		public int etc;
	}

	class Program
	{
		static string ProjectName = "ParseEvsXmlElectionDataForPimaCountyAZ";

		static string LocationBeingStudied = "PimaCountyAZ";

		static string LocationBeingStudiedClearly = "Pima County Arizona";

		static string gitHubRepositoryURL = "https://github.com/IneffablePerformativity/" + ProjectName;

		static string gitHubRepositoryShortened = "https://bit.ly/2JFg8rg";
		
		// Now generated on the fly as I mark the Xs:
		static string finalConclusion = "FWIW, The CISR Rumor Control page says undervote (here 'Bonus') is not a sign of fraud. YMMV.";
		static string flipsideConclusion = "FWIW, The CISR Rumor Control page says undervote (here 'Bonus') is not a sign of fraud. YMMV.";

		static bool discardStraightPartyVotes = false; // affects bonus, csv, more than plot
//		static bool discardStraightPartyVotes = true; // affects bonus, csv, more than plot

		// Straight-Party votes add to all (POTUS,SENUS,REPUS)
		
		static bool useConceptOfStraightPartyVotes = false; // only affects messages

		// If this worked, it should make the 2 POTUS votes coincide. Yes, it does work.
		// Ah, but the divisor change affects Potus and Other: no net effect on Bonuses.

		static bool omitEtcPotusFromTotalBallots = false;
		
		// Canonical s/b REP, as they race every 2 years, and each voter sees one.
		// I see that in some locations, there are many HOUSE races that are uncontested.

		// Milwaukee County WI has no senators, only CONGRESS to compare here:
		// Pima County AZ blew up the plot using Congress, must study.
		// Oh, AZ data included minor races, new input code must exclude.
		static int howOther = 1; // AVERAGE - 1=average of Sen+Rep, 2=Sen, 3=Rep
//		static int howOther = 2; // SENATE - 1=average of Sen+Rep, 2=Sen, 3=Rep
//		static int howOther = 3; // CONGRESS - 1=average of Sen+Rep, 2=Sen, 3=Rep

		// This should de-noise plot.
		// using 100 here eliminates about 90 CSV rows, leaves about 1000 rows in Oakland.Co.
		
//		static int someMinimumTotalBallots = 500;
//		static int someMinimumTotalBallots = 200;
//		static int someMinimumTotalBallots = 2000; // big counts in GA, drops ~10 counties
//		static int someMinimumTotalBallots = 1000; // 1000 knocks off 66 of 743 rows in Pima
		static int someMinimumTotalBallots = 0;
		
		// The LEFT of graph shows nothing interesting (sorted by Republicanism).
		// This should chop some of left off so i need not stretch image so much.

//		static int someMinimumRepublicanism = 400000; // 40%
//		static int someMinimumRepublicanism = 200000; // 20%
		static int someMinimumRepublicanism = 0; // omits none
		
		// but wait, having nearly zero "other" of either party to divide by is disaster.

		static int someMinOtherBothParties = 1; // c'mon man, zero is bad
//		static int someMinOtherBothParties = 50; // also will eliminate any uncontested races.
//		static int someMinOtherBothParties = 5; // also will eliminate any uncontested races.
//		static int someMinOtherBothParties = 0; // omits none
		
		// plot abscissa orderings are editted in code.
		// By this principle, unless changed in code:

		static string orderingPrinciple = "ascending Republicanism (red area)"; // edit code below to modify.

		// well, why not have a choice up here?

//		static int howOrdering = 0; // 0 gives the default just stated, 1-7 change it.
//		static int howOrdering = 7; // 7 is ballots
		static int howOrdering = 8; // 8 is net bonus

		static string insertLeftSortedParenthesis = ""; // only for certain methods
		static string insertRightSortedParenthesis = ""; // only for certain methods
		
		// in VA, the blue bars plotted way over 100%, outside the graph.
		// Fix so if non-POTUS total is larger than POTUS total, use that.

		static bool raiseTotalBallotsUpToOtherVotes = false;
		
		// Grains are smallest locality, perhaps a Ward, county, precinct...

		const string GrainTag = "Precinct"; // XML tag name e.g., Precinct, County, Ward, etc.

		
		
		// inputting phase

		
		static string inputDir = @"C:\A\SharpDevelop\" + ProjectName;
		
		// for the XML version, not used here:
		
		//static string inputXmlFileName = LocationBeingStudied + "-detail.xml"; // sitting there

		// re-adding the HTML input variables and method at the bottom of this file.
		
		// for the HTML version, used here:

		static string inputTextFileName = "ArizonaExportByPrecinct_110320.txt"; // sitting there

		
		// This is the top-level store of all grains, held by locality name
		
		static Dictionary<string,Grain> grains = new Dictionary<string,Grain>();

		
		// ====================
		// Analyzing Phase
		// ====================

		
		// Literal strings for N.B.: *TWO*switch()s, qv.


		// if applicable, adds REP or DEM count to all contests
		//const string contestStraight = "Straight Party Ticket";
		// this came back in SC:
		const string contestStraight1 = "STRAIGHT PARTY 1"; // none here...

		
		// Sum of all party's votes for POTUS race in each grain
		// become my approximation of the total Ballots in grain.
		// New fix: use max(Potus,non-Potus) votes in each grain.
		const string contestPotus = "PRESIDENTIAL ELECTORS";
		//contest: PRESIDENTIAL ELECTORS
		//contest: UNITED STATES SENATOR
		//contest: U.S. REPRESENTATIVE IN CONGRESS, DIST. 1
		//contest: U.S. REPRESENTATIVE IN CONGRESS, DIST. 2
		//contest: U.S. REPRESENTATIVE IN CONGRESS, DIST. 3

		
		// zero, one, or two
		const string contestSenus1 = "UNITED STATES SENATOR";

		
		// I read that ALL VOTERS get to vote in one Congress contest.
		// I only see Potus & Repus in PA, so will use a default case.
		// Again, in VA I can use default case.
		const string contestRepus1 = "U.S. REPRESENTATIVE IN CONGRESS, DIST. 1";
		const string contestRepus2 = "U.S. REPRESENTATIVE IN CONGRESS, DIST. 2";
		const string contestRepus3 = "U.S. REPRESENTATIVE IN CONGRESS, DIST. 3";
		
		
		// outputting phase
		
		

		
		static string DateTimeStr = DateTime.Now.ToString("yyyyMMddTHHmmss");
		
		
		static string DateStampPlot = DateTime.Now.ToString("yyyy-MM-dd");

		// Log outputs exploratory, debug data:

		static string logFilePath = @"C:\A\" + DateTimeStr + "_" + ProjectName + "_log.txt";
		static TextWriter twLog = null;

		// this sum is to plot widths of each grain
		// proportional to its population = ballots.
		
		static int SumOfGrainTotalBallots = 0;

		// a favorite output idiom

		static void say(string msg)
		{
			if(twLog != null)
				twLog.WriteLine(msg);
//			else
			Console.WriteLine(msg);
		}

		// Csv outputs voting data to visualize in Excel

		static string csvFilePath = @"C:\A\" + DateTimeStr + "_" + ProjectName + "_csv.csv";
		static TextWriter twCsv = null;

		static void csv(string msg)
		{
			if(twCsv != null)
				twCsv.WriteLine(msg);
		}

		// this cleans csv output text

		static Regex regexNonAlnum = new Regex(@"\W", RegexOptions.Compiled);
		
		static char [] caComma = { ',' };

		
		// Png outputs my own bitmap for fine grained control of data display:

		static string pngFilePath = @"C:\A\" + DateTimeStr + "_" + ProjectName + "_png.png";

		
		// Affects pen width choices!

		// Stretch the plot X direction due to so many data:
		const int stretchFactor = 2; // need 4 to see any details
		
		const int imageSizeX = stretchFactor * 1920 * 2;
		const int imageSizeY = 1080 * 2;
		const int nBorder = 100 * 2; // keep mod 10 == 0, now also mod 20 == 0
		
		const int plotSizeX = imageSizeX - 2 * nBorder;
		const int plotSizeY = imageSizeY - 2 * nBorder + 1; // so plotSizeY mod 10 == 1 for 11 graticules
		

		// When I fixed the 'four' mistake, 2020-11-27, bonuses jumped up!
		// Old 11 graticules with full scale +-5% doesn't cut it any more.

		static bool do21gratsfspm10 = true;

		// on PPM, plot full scale of 1M = 100% (of total votes in grain)
		// if desire bonus plot full scale = +/-5% then scale up by 10.
		// if desire bonus plot full scale = +/-10% then scale up by 5.
		
		static int scaleFactor = (do21gratsfspm10 ? 5 : 10);
		
		// N.B. At current 21 graticules, 200K = 4%
		// but at 11 graticules, 200K = 2%
		
		static int XPct = 1;
		static int XMarksTheSpotThreshold = XPct * (do21gratsfspm10 ? 50000 : 100000);

		static int countTheBidenXMarksTheSpot = 0;
		static int effectOfBidenBonusOnVotes = 0;
		
		// 2020-11-30 it is only appropriate to count and display the meagre flip side:
		static int countTheTrumpXMarksTheSpot = 0;
		static int effectOfTrumpBonusOnVotes = 0;
		
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			
			// TODO: Implement Functionality Here
			
			using(twLog = File.CreateText(logFilePath))
				using(twCsv = File.CreateText(csvFilePath))
			{
				try { doit(); } catch(Exception e) {say(e.ToString());}
			}

			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}

		
		static void doit()
		{
			// Phase one -- Inputting data
			
			if(useConceptOfStraightPartyVotes)
			{
				say("discardStraightPartyVotes = " + discardStraightPartyVotes);
			}
			
			
			// inputDetailXmlFile(); // very commonly data set
			
			
			inputPimaDetailXmlFile();
			

			//diagnosticDump(); // optional, not needed
			
			
			// Phase two -- Analyze data
			
			LetsRaceTowardTheDesideratum();

			
			// Phase three -- Output data
			
			// I scale [0.0% to 1.0%] into ppm = Parts Per Million [0 to 1000000].
			// all ppm are computed with divisor = totalBallots in grain locality.

			if(twCsv != null)
				csv("ordering,ppmRepPotus,ppmDemPotus,1M-ppmDemPotus,ppmRepOther,ppmDemOther,1M-ppmDemOther,BonusToTrump,BonusToBiden,totalBallots,locality");

			OutputTheCsvResults();
			
			OutputTheStatistics();

			OutputThePlotResults();

			// I just realized, but in too big a hurry to reverse it,
			// but the 3 OMITTING factors affect CSV as well as plot.
			say("Note that all items omitted above affect the CSV as well as the PNG.");

		}

		

		// "Clarity" XML version of Front-End inputter to application:



		static void inputDetailXmlFile()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(Path.Combine(inputDir,inputXmlFileName));

			XmlNode root = doc.DocumentElement;

			// XML descent

			XmlNodeList xnlContests = root.SelectNodes("Contest");
			foreach(XmlNode xnContest in xnlContests)
			{
				string contest = xnContest.Attributes["text"].Value;

				say(contest); // to fix const strings

				// Having just run through all to see the lay of it,
				// Only process contest of interest to my statistic.

				switch(contest)
				{
					case contestStraight1:
						if(discardStraightPartyVotes)
							continue;
						break; // Else, Process this category

					case contestPotus:
						break; // Process this category

					case contestSenus1:
						break; // Process this category

					case contestRepus1:
					case contestRepus2:
					case contestRepus3:
						break; // Process this category

					default:
						continue; // discard all others
				}

				XmlNodeList xnlChoices = xnContest.SelectNodes("Choice");
				foreach(XmlNode xnChoice in xnlChoices)
				{
					string choice = xnChoice.Attributes["text"].Value;

					string party = "";

					// This worked in the Oakland County, and West VA verion of app:

					try { party = xnChoice.Attributes["party"].Value; } catch(Exception) { };


					// This is the code for GEORGIA, came from inspecting choice string:

					//					if(choice.Contains("(Rep)"))
					//						party = "REP";
					//					if(choice.Contains("(Dem)"))
					//						party = "DEM";

					// 1. Real votes
					XmlNodeList xnlGrains = xnChoice.SelectNodes("VoteType/" + GrainTag);
					foreach(XmlNode xnGrain in xnlGrains)
					{
						string grain = xnGrain.Attributes["name"].Value;
						string votes = xnGrain.Attributes["votes"].Value;

						ponderInput(contest, choice, party, grain, votes);
					}
				}

				// Having seen it, now not needed
				//
				//{
				//	// 2. Residual votes
				//	// <VoteType name="Undervotes"...
				//	XmlNodeList xnlGrains = xnContest.SelectNodes("VoteType[@name='Overvotes' or @name='Undervotes']/" + GrainTag);
				//	foreach(XmlNode xnGrain in xnlGrains)
				//	{
				//		string grain = xnGrain.Attributes["name"].Value;
				//		string votes = xnGrain.Attributes["votes"].Value;
				//
				//		ponderInput(contest, "Residual", "", grain, votes);
				//	}
				//}
			}
			// Wow. That was way easier.
		}

		
		static void ponderInput(string contest, string choice, string party, string grain, string votes)
		{
			say(contest + "::" + choice + "::" + party + "::" + grain + "::" + votes);
			
			int nVotes = int.Parse(votes);
			
			// Hang novel grain names into grains dict as encountered.
			
			Grain thisGrain = null;
			if(grains.ContainsKey(grain))
				thisGrain = grains[grain];
			else
				grains.Add(grain, thisGrain = new Grain(grain));
			
			// Hang novel contest names into contests sub-dict as encountered.

			Contest thisContest = null;
			if(thisGrain.contests.ContainsKey(contest))
				thisContest = thisGrain.contests[contest];
			else
				thisGrain.contests.Add(contest, thisContest = new Contest());

			// Hang novel choice names into choices sub-sub-dict as encountered.
			
			// I save the REP, DEM, ETC into choice, but need no sub-branches.
			
			Choice thisChoice = null;
			if(thisContest.choices.ContainsKey(choice))
				thisChoice = thisContest.choices[choice];
			else
				thisContest.choices.Add(choice, thisChoice = new Choice(party));
			
			// count these votes into the hanging grain-contest-choice(party)

			thisChoice.votes += nVotes;
		}

		
		/*
		 * I think having made the effort to parse and count Residual counts,
		 * all contests->...->grains->votes should sum to the identical ballotsCast.
		 * 
		 * <VoterTurnout totalVoters="1035172" ballotsCast="775379" voterTurnout="74.90">
		 * 
		 * No, I think not now:
		 * Some local races have fewer of the statewide ballots,
		 * but I did get some lines to sum up within a Grain:
		 * 
		 * input, under TotalVotes:
		 * <Precinct name="Addison Township, Precinct 1" totalVoters="1930" ballotsCast="1590" ...
		 * 
		 * versus output:
		 * Addison Township, Precinct 1::Straight Party Ticket::1590
		 * Addison Township, Precinct 1::Electors of President and Vice-President of the United States::1590
		 * Addison Township, Precinct 1::United States Senator::1590
		 * 
		 * however, I see some near-multiples and multiples too:
		 * Addison Township, Precinct 1::Member of the State Board of Education::3179
		 * Addison Township, Precinct 1::Regent of the University of Michigan::3180
		 * 
		 * So I think my diagnostic failed to sum-up to a high enough level...
		 * 
		 * Fixed it.
		 * 
		 * Did not snag all of Federals; Many others are don't cares:
		 * GOOD TOTAL: Straight Party Ticket
		 * GOOD TOTAL: Electors of President and Vice-President of the United States
		 * GOOD TOTAL: United States Senator
		 * hand work:
		 * Line 1: Representative in Congress 8th District = 164088
		 * Line 26: Representative in Congress 9th District = 137999
		 * Line 49: Representative in Congress 11th District = 293394
		 * Line 156: Representative in Congress 14th District = 179898
		 * 164088 + 137999 + 293394  + 179898 = 775379, matches ballotsCast = 775379
		 */

		
		static void diagnosticDump()
		{
			// because of the inverted order of grain<-->contest, sum up:
			Dictionary<string,int> contestSums = new Dictionary<string, int>();

			foreach(KeyValuePair<string,Grain>kvp1 in grains)
			{
				string g = kvp1.Key;
				Grain grain = kvp1.Value;
				foreach(KeyValuePair<string,Contest>kvp2 in grain.contests)
				{
					string c = kvp2.Key;
					Contest contest = kvp2.Value;
					int sumVotes = 0;
					foreach(KeyValuePair<string,Choice>kvp3 in contest.choices)
					{
						string n = kvp3.Key;
						Choice choice = kvp3.Value;
						sumVotes += choice.votes;
					}
					if(contestSums.ContainsKey(c))
						contestSums[c] += sumVotes;
					else
						contestSums.Add(c, sumVotes);
				}
			}
			
			//int nGood = 0;
			//int nBad = 0;
			//foreach(KeyValuePair<string,int>kvp in contestSums)
			//{
			//	if(kvp.Value == ballotsCast)
			//	{
			//		// say("GOOD TOTAL: " + kvp.Key); // see which 15?
			//		nGood++;
			//	}
			//	else
			//	{
			//		// say(kvp.Key + " = " + kvp.Value); // How bad?
			//		nBad++;
			//	}
			//}
			// say("nGood = " + nGood);
			// say("nBad = " + nBad);
			//nGood = 15
			//nBad = 260
			// I'll take that as a big win
		}
		

		// This will collect output lines to sort for csv.
		// After sort, re-parse column data to draw graph.
		
		static List<string> csvLines = new List<string>();

		// This straight (but biased) line is NOT the right clue:
		//
		// My God! Thank You, My God!
		// The Excel plot reveals a VERY STRAIGHT LINE
		// revealing the algorithm favoring Joe Biden.
		// So that I must see and report mean, std dev.
		//
		// That 1.5% was in some prior location's data.
		// for generality, output both statistics.
		//
		// (But PimaCountyAZ had a sloping line.)
		// But output these as a signed percentage:
		//
		// Now rather, having fixed my 'four' mistake,
		// I expect to see Bonuses for Biden jump out!
		

		// These hold the counts for mean, std dev:
		
		static List<int> BidenBonuses = new List<int>();
		static List<int> TrumpBonuses = new List<int>();
		
		
		// Isaiah 41:15 “Behold, I will make thee a new sharp threshing instrument having teeth: thou shalt thresh the mountains, and beat them small, and shalt make the hills as chaff.”
		
		
		static void LetsRaceTowardTheDesideratum()
		{
			StringBuilder sb = new StringBuilder();
			
			// Grains are plotted along the Abscissa, the X axis.
			// Each outer loop can prepare one csv/plot data out.
			// That is because I already learned the ballotsCast.
			// No rather, compute a total ballots for each grain.

			// I will learn contest long before I learn party.

			// In Version one, that is, before 2020-11-27,
			// I wrongly placed the 'thisFour' ahead of loop.
			// In Correct version two, on/after 2020-11-28,
			// I moved the 'thisFour' creation into the loop.
			
			foreach(KeyValuePair<string,Grain>kvp1 in grains)
			{

				
				ShrodingersCat [] thisFour = new ShrodingersCat[4];

				thisFour[0] = new ShrodingersCat(); // Straight -- no effect if not applicable
				thisFour[1] = new ShrodingersCat(); // President
				thisFour[2] = new ShrodingersCat(); // Senate
				thisFour[3] = new ShrodingersCat(); // Congress

				int which = 0;


				string g = kvp1.Key; // county, ward, precinct name
				Grain grain = kvp1.Value;

				// Middle loop splits up 2 contest types: (POTUS vs. LOWER)
				// Well, actually, into the four cats I created just above.

				foreach(KeyValuePair<string,Contest>kvp2 in grain.contests)
				{
					string c = kvp2.Key;
					Contest contest = kvp2.Value;

					switch(c)
					{
						case contestStraight1:
							// I should add to all possible races.
							// These will only dilute the symptom.
							which = 0;
							break;

						case contestPotus:
							which = 1;
							break;

						case contestSenus1:
							which = 2;
							break;

						case contestRepus1:
						case contestRepus2:
						case contestRepus3:
							which = 3;
							break;
					}

					// Inner loop splits up 2 choice types: (REP vs. DEM).
					
					// In Oakland County MI data, these came as XML attributes:

					foreach(KeyValuePair<string,Choice>kvp3 in contest.choices)
					{
						string n = kvp3.Key;
						Choice choice = kvp3.Value;
						
						switch(choice.party)
						{
							case "DEM":
								thisFour[which].dem += choice.votes;
								break;
							case "REP":
								thisFour[which].rep += choice.votes;
								break;
							default:
								thisFour[which].etc += choice.votes;
								break;
						}
					}
				}

				// build a CSV output line for this grain
				
				
				// fyi: csv("ordering,ppmRepPotus,ppmDemPotus,1M-ppmDemPotus,ppmRepOther,ppmDemOther,1M-ppmDemOther,BonusToTrump,BonusToBiden,totalBallots,locality");

				// sum up VOTES:
				int RepPotus = thisFour[0].rep + thisFour[1].rep;
				int DemPotus = thisFour[0].dem + thisFour[1].dem;
				int EtcPotus = thisFour[0].etc + thisFour[1].etc;
				
				// In Oakland County Michigan, there was an "undervote"
				// i.e., apparent bonus of about 1-2% for both sides.
				
				// BTW, CISA rumor control says that that is normal:
				// https://www.cisa.gov/rumorcontrol
				
				// But in this case, I summed EtcPotus into totalBallots,
				// whereas the sum for Other races sums only Rep and Dem.
				
				// therefore, optionally, omitEtcPotusFromTotalBallots.
				
				int totalBallots = RepPotus + DemPotus + EtcPotus;
				if(omitEtcPotusFromTotalBallots)
				{
					totalBallots = RepPotus + DemPotus; // omit  + EtcPotus
					
				}
				
				// New problem in Virginia:
				// The blue bars plotted way over 100%, falling outside the graph.
				// Fix so if non-POTUS total is larger than POTUS total, use that.\
				//
				// Ahh, but wait! This just came in over GAB.com:
				//https://10ztalk.com/2020/12/01/kraken-on-steroids-sidney-powell-claims-massive-voter-fraud-in-virginia-warns-see-you-soon/
				//
				//Kraken On Steroids: Sidney Powell Claims Massive Voter Fraud in Virginia, Warns “See You Soon”
				//
				// This blue graph may actually be a clue to that.
				// It seemed like a good idea, but make an option.
				if(raiseTotalBallotsUpToOtherVotes)
				{
					switch(howOther) // 1=average of Sen+Rep, 2=Sen, 3=Rep
					{
						case 1:
							// compute average totals:
							int AvgTotals = 0;
							AvgTotals += thisFour[0].rep + thisFour[2].rep;
							AvgTotals += thisFour[0].dem + thisFour[2].dem;
							AvgTotals += thisFour[0].etc + thisFour[2].etc;
							AvgTotals += thisFour[0].rep + thisFour[3].rep;
							AvgTotals += thisFour[0].dem + thisFour[3].dem;
							AvgTotals += thisFour[0].etc + thisFour[3].etc;
							AvgTotals /= 2;
							
							if(totalBallots < AvgTotals)
							{
								totalBallots = AvgTotals;
							}
							break;
						case 2:
							// compute senate totals:
							int senTotals = 0;
							senTotals += thisFour[0].rep + thisFour[2].rep;
							senTotals += thisFour[0].dem + thisFour[2].dem;
							senTotals += thisFour[0].etc + thisFour[2].etc;
							if(totalBallots < senTotals)
							{
								totalBallots = senTotals;
							}
							break;
						case 3:
							// compute house totals:
							int RepTotals = 0;
							RepTotals += thisFour[0].rep + thisFour[3].rep;
							RepTotals += thisFour[0].dem + thisFour[3].dem;
							RepTotals += thisFour[0].etc + thisFour[3].etc;
							
							if(totalBallots < RepTotals)
							{
								totalBallots = RepTotals;
							}
							break;
					}
				}

				// prevent divide by zero:
				// also declutter noise.
				if(totalBallots < 1 + someMinimumTotalBallots)
				{
					say("OMITTING where totalBallots == " + totalBallots + " in " + g);
				}
				else
				{
					// 'Other' meaning non-POTUS
					
					// factor in someMinOtherBothParties
					bool okay = true;
					
					int RepOther = -1;
					int DemOther = -1;
					switch(howOther) // 1=average of Sen+Rep, 2=Sen, 3=Rep
					{
						case 1:
							// was:
							// RepOther = thisFour[0].rep + (thisFour[2].rep + thisFour[3].rep) / 2;
							// DemOther = thisFour[0].dem + (thisFour[2].dem + thisFour[3].dem) / 2;

							// apply minimum to both sides:
							{
								int RepOther1 = thisFour[0].rep + thisFour[2].rep;
								int DemOther1 = thisFour[0].dem + thisFour[2].dem;
								if(RepOther1 < someMinOtherBothParties
								   || DemOther1 < someMinOtherBothParties)
									okay = false;
								int RepOther2 = thisFour[0].rep + thisFour[3].rep;
								int DemOther2 = thisFour[0].dem + thisFour[3].dem;
								if(RepOther2 < someMinOtherBothParties
								   || DemOther2 < someMinOtherBothParties)
									okay = false;
								RepOther = (RepOther1 + RepOther2)/2;
								DemOther = (DemOther1 + DemOther2)/2;
							}
							break;
						case 2:
							RepOther = thisFour[0].rep + thisFour[2].rep;
							DemOther = thisFour[0].dem + thisFour[2].dem;
							if(RepOther < someMinOtherBothParties
							   || DemOther < someMinOtherBothParties)
								okay = false;
							break;
						case 3:
							RepOther = thisFour[0].rep + thisFour[3].rep;
							DemOther = thisFour[0].dem + thisFour[3].dem;
							if(RepOther < someMinOtherBothParties
							   || DemOther < someMinOtherBothParties)
								okay = false;
							break;
					}
					
					if( ! okay)
					{
						say("OMITTING where someMinOtherBothParties (" + someMinOtherBothParties + ") was not met in " + g);
					}
					else
					{
						
						int ppmRepPotus = (int)(1000000L * RepPotus / totalBallots);
						int ppmDemPotus = (int)(1000000L * DemPotus / totalBallots);

						int ppmRepOther = (int)(1000000L * RepOther / totalBallots);
						int ppmDemOther = (int)(1000000L * DemOther / totalBallots);
						
						int ppmTrumpBonus = ppmRepPotus - ppmRepOther;
						int ppmBidenBonus = ppmDemPotus - ppmDemOther;
						
						
						// Chop off the left of graph,
						// when no clear fraud effect.
						if(ppmRepPotus < someMinimumRepublicanism)
						{
							say("OMITTING where ppmRepPotus < someMinimumRepublicanism in " + g);
						}
						else
						{
							SumOfGrainTotalBallots += totalBallots;

							// these are signed, around zero, but scaled in PPM

							BidenBonuses.Add(ppmBidenBonus); // for later mean, std dev
							TrumpBonuses.Add(ppmTrumpBonus); // for later mean, std dev

							
							int bonusToTrump = 500000 + scaleFactor * ppmTrumpBonus; // both plot up==positive
							int bonusToBiden = 500000 + scaleFactor * ppmBidenBonus; // both plot up==positive
							
							// This ordering method follows (only Other, of only Rep) no Dem, no Potus,
							// as Original Milwaukee article says shifts proportional to Republicanism.
							
							int ppmOrdering = ppmRepOther;
							
							
							// These make valuable alternate plots for visualization.
							
							switch(howOrdering)
							{
								case 1:
									{
										ppmOrdering = bonusToBiden;
										orderingPrinciple = "ascending BonusToBiden";
									}
									break;
								case 2:
									{
										ppmOrdering = bonusToTrump;
										orderingPrinciple = "ascending BonusToTrump";
									}
									break;
								case 3:
									{
										ppmOrdering = ppmDemOther;
										orderingPrinciple = "ascending ppmDemOther";
									}
									break;
								case 4:
									{
										ppmOrdering = ppmRepOther;
										orderingPrinciple = "ascending ppmRepOther";
									}
									break;
								case 5:
									{
										ppmOrdering = ppmDemPotus;
										orderingPrinciple = "ascending ppmDemPotus";
									}
									break;
								case 6:
									{
										ppmOrdering = ppmRepPotus;
										orderingPrinciple = "ascending ppmRepPotus";
									}
									break;
								case 7:
									{
										ppmOrdering = totalBallots;
										orderingPrinciple = "ascending totalBallots";
									}
									break;
								case 8:
									{
										// this naive math goes negative, sorts wrong:
										// ppmOrdering = bonusToBiden - bonusToTrump;
										// this still can exceed range, go negative:
										// ppmOrdering = bonusToBiden - bonusToTrump + 500000;
										ppmOrdering = (bonusToBiden - bonusToTrump) / 2 + 500000;
										orderingPrinciple = "ascending (BonusToBiden (green) minus BonusToTrump (orange))";

										insertLeftSortedParenthesis = "(SORTED TO THE LEFT) ";
										insertRightSortedParenthesis = "(SORTED TO THE RIGHT) ";
									}
									break;
							}

							// that's the data, create the CSV line

							
							sb.Clear();
							// field[0]
							sb.Append(ppmOrdering.ToString().PadLeft(7)); sb.Append(',');

							// field[1]
							sb.Append(ppmRepPotus.ToString().PadLeft(7)); sb.Append(',');
							// field[2]
							sb.Append(ppmDemPotus.ToString().PadLeft(7)); sb.Append(',');
							sb.Append((1000000 - ppmDemPotus).ToString().PadLeft(7)); sb.Append(',');

							// field[4]
							sb.Append(ppmRepOther.ToString().PadLeft(7)); sb.Append(',');
							// field[5]
							sb.Append(ppmDemOther.ToString().PadLeft(7)); sb.Append(',');
							sb.Append((1000000 - ppmDemOther).ToString().PadLeft(7)); sb.Append(',');

							// field[7]
							sb.Append(bonusToTrump.ToString().PadLeft(7)); sb.Append(',');
							// field[8]
							sb.Append(bonusToBiden.ToString().PadLeft(7)); sb.Append(',');

							sb.Append(totalBallots.ToString().PadLeft(7)); sb.Append(',');

							sb.Append(regexNonAlnum.Replace(grain.locality, ""));

							csvLines.Add(sb.ToString());
						}
					}
				}
			}
		}
		
		static void OutputTheCsvResults()
		{
			csvLines.Sort();
			
			foreach(string line in csvLines)
				csv(line);
		}
		
		static void OutputTheStatistics()
		{
			// this was more relevant when my 'four' error masked big Bonus variations.
			string possibleConclusionText = "";
			
			// Do once for Biden
			{
				double mean = 0;
				double stdDev = 0;
				if(BidenBonuses.Count > 0)
				{
					// BidenBonuses broke the bank in AZ!
					//int sum = 0;
					long sum = 0;
					foreach(int i in BidenBonuses) // signed around zero, scaled*1M
						sum += i;
					mean = (double)sum / BidenBonuses.Count;
					
					double sumSquares = 0.0;
					foreach(int i in BidenBonuses)
						sumSquares += (mean-i)*(mean-i);
					stdDev = Math.Sqrt(sumSquares / BidenBonuses.Count);
				}
				// divide by 100 first to keep 2 dec plcs in pct
				int imean = (int)Math.Round(mean/100);
				string sign = (imean<0?"MINUS ":"PLUS ");
				imean = Math.Abs(imean);
				int istdDev = (int)Math.Round(stdDev/100);
				double dmean = imean / 100.0; // now as Percentage
				double dstdDev = istdDev / 100.0; // now as Percentage

				say("Across the " + BidenBonuses.Count + " localities of " + LocationBeingStudied
				    + ", the BIDEN vote differs from other DEMOCRAT races as: mean = "
				    + sign + dmean + "% of total votes in locality, with standard deviation = " + dstdDev + "%.");
				possibleConclusionText += " BIDEN BONUS = " + sign + dmean + "%, StdDev " + dstdDev + ";";
			}
			
			// Do once for Trump
			{
				double mean = 0;
				double stdDev = 0;
				if(TrumpBonuses.Count > 0)
				{
					// BidenBonuses broke the bank in AZ!
					//int sum = 0;
					long sum = 0;
					foreach(int i in TrumpBonuses) // signed around zero, scaled*1M
						sum += i;
					mean = (double)sum / TrumpBonuses.Count;
					
					double sumSquares = 0.0;
					foreach(int i in TrumpBonuses)
						sumSquares += (mean-i)*(mean-i);
					stdDev = Math.Sqrt(sumSquares / TrumpBonuses.Count);
				}
				// divide by 100 first to keep 2 dec plcs in pct
				int imean = (int)Math.Round(mean/100);
				string sign = (imean<0?"MINUS ":"PLUS ");
				imean = Math.Abs(imean);
				int istdDev = (int)Math.Round(stdDev/100);
				double dmean = imean / 100.0; // now as Percentage
				double dstdDev = istdDev / 100.0; // now as Percentage
				
				say("Across the " + TrumpBonuses.Count + " localities of " + LocationBeingStudied
				    + ", the TRUMP vote differs from other REPUBLICAN races as: mean = "
				    + sign + dmean + "% of total votes in locality, with standard deviation = " + dstdDev + "%.");
				possibleConclusionText += " TRUMP BONUS = " + sign + dmean + "%, StdDev " + dstdDev;
			}
			say("possibleConclusionText" + possibleConclusionText);
		}
		
		
		// Habakkuk 2:2 "And the Lord answered me, and said, Write the vision, and make it plain upon tables, that he may run that readeth it."

		
		static void OutputThePlotResults()
		{
			Bitmap bmp = new Bitmap(imageSizeX, imageSizeY);
			bmp.SetResolution(92, 92);
			Graphics gBmp = Graphics.FromImage(bmp);
			
			Brush whiteBrush = new SolidBrush(Color.White);
			gBmp.FillRectangle(whiteBrush, 0, 0, imageSizeX, imageSizeY); // x,y,w,h

			// again:csv("ordering,ppmRepPotus,ppmDemPotus,1M-ppmDemPotus,ppmRepOther,ppmDemOther,1M-ppmDemOther,BonusToTrump,BonusToBiden,totalBallots,locality");
			
			// goggled the official party colors.
			// Rep Red
			//RGB: 233 20 29
			//HSL: 357° 84% 50%
			//Hex: #E9141D
			// Dem Blue
			//RGB: 0 21 188
			//HSL: 233° 100% 37%
			//Hex: #0015BC

			// There are SO many X items in Pima County AZ,
			// that I must reduce the red/blue line widths.
//			const int votesLineWidth = 12; // at 1920 * 2
			const int votesLineWidth = 6; // at 1920 * 2
			Pen redTrumpLinePen = new Pen(Color.FromArgb(233, 20, 29), votesLineWidth);
			Pen blueBidenLinePen = new Pen(Color.FromArgb(0, 21, 188), votesLineWidth);

			// ... and raise the bonus width to see them:
//			const int bonusLineWidth = 8; // at 1920 * 2
			const int bonusLineWidth = 12; // at 1920 * 2
			Pen GreenBidenBonusLinePen = new Pen(Color.Green, bonusLineWidth);
			Pen orangeTrumpBonusLinePen = new Pen(Color.Orange, bonusLineWidth);

			Pen blackXPen = new Pen(Color.Black, 3); // at 1920* 2
			Pen whiteXPen = new Pen(Color.White, 3); // at 1920* 2
			Pen blackGraticulePen = new Pen(Color.Black, 6); // at 1920* 2
			Pen blackFatCenterLinePen = new Pen(Color.Black, 16); // at 1920* 2

			// Wiki: Pastels in HSV have high value and low to intermediate saturation.
			// Made my color conversions at https://serennu.com/colour/hsltorgb.php
			//Trying Pastel HSL: 357 40 90
			//HSL: 357° 40% 90%
			//RGB: 240 219 220
			//Hex: #F0DBDC
			//Trying Pastel HSL: 233 40 90
			//HSL: 233° 40% 90%
			//RGB: 219 222 240
			//Hex: #DBDEF0
			
			Brush redRepublicansBrush = new SolidBrush(Color.FromArgb(240, 219, 220));
			Brush blueDemocratsBrush = new SolidBrush(Color.FromArgb(219, 222, 240));

			// there are two similar looking code blocks, for areas, and for lines.

			// plot the areas

			{
				// gBmp.FillRectangle(); // brush,x,y,w,h

				// SumOfGrainTotalBallots must fit inside plotSizeX.
				// The ballots in grain is last-1 field of csv data.
				
				// ascends during loops
				float leftOfBar = nBorder;

				// factor in the border offsets ahead of loop
				int zeroForDescendingY = nBorder;

				//int zeroForBonusesY = (1 + 5*plotSizeY/10) + nBorder;
				int zeroForAscendingY = plotSizeY + nBorder;

				
				// draw areas first
				
				foreach(string line in csvLines)
				{
					string[] fields = line.Split(caComma);
					int grainBallots = int.Parse(fields[9]);
					float barWidth = (float)plotSizeX * grainBallots / SumOfGrainTotalBallots;

					int ppmRepOther = int.Parse(fields[4]);
					int ppmDemOther = int.Parse(fields[5]);
					float RepHeight = (float)plotSizeY * ppmRepOther / 1000000;
					float DemHeight = (float)plotSizeY * ppmDemOther / 1000000;
					gBmp.FillRectangle(blueDemocratsBrush, leftOfBar, zeroForDescendingY, barWidth, DemHeight);
					gBmp.FillRectangle(redRepublicansBrush, leftOfBar, zeroForAscendingY - RepHeight, barWidth, RepHeight);
					
					leftOfBar += barWidth;
				}
			}

			// draw graticules atop areas

			{
				int gratStepSize;
				if(do21gratsfspm10)
					gratStepSize = plotSizeY/20;
				else
					gratStepSize = plotSizeY/10;

				// Draw the eleven 10% or 21 5% horizontal graticule lines:
				for(int y = 1; y <= plotSizeY; y += gratStepSize)
				{
					if(y == 1 + 5*plotSizeY/10)
						gBmp.DrawLine(blackFatCenterLinePen, nBorder, nBorder + y, nBorder + plotSizeX, nBorder + y); // x1, y1, x2, y2
					else
						gBmp.DrawLine(blackGraticulePen, nBorder, nBorder + y, nBorder + plotSizeX, nBorder + y); // x1, y1, x2, y2
				}

				// Draw two vertical boundary lines
				for(int x = 0; x <= plotSizeX; x += plotSizeX)
				{
					// looked like high water trouser legs. Fudging + 1:
					gBmp.DrawLine(blackGraticulePen, nBorder + x, nBorder, nBorder + x, nBorder + plotSizeY + 1); // x1, y1, x2, y2
				}
			}
			
			// plot the varying data lines
			
			{
				// ascends during loops
				float leftOfBar = nBorder;

				// factor in the border offsets ahead of loop
				int zeroForDescendingY = nBorder;
				int zeroForBonusesY = (1 + 5*plotSizeY/10) + nBorder;
				int zeroForAscendingY = plotSizeY + nBorder;

				// these are to connect line segments
				float priorXreached = 0;
				float priorTrumpY = 0;
				float priorBidenY = 0;
				float priorTrumpBonusY = 0;
				float priorBidenBonusY = 0;
				
				foreach(string line in csvLines)
				{
					string[] fields = line.Split(caComma);
					int grainBallots = int.Parse(fields[9]);
					float barWidth = (float)plotSizeX * grainBallots / SumOfGrainTotalBallots;

					int ppmRepPotus = int.Parse(fields[1]);
					int ppmDemPotus = int.Parse(fields[2]);
					int bonusToTrump = int.Parse(fields[7]) - 500000; // was scaled up *10 and shifted +500K
					int bonusToBiden = int.Parse(fields[8]) - 500000; // was scaled up *10 and shifted +500K

					float RepHeight = (float)plotSizeY * ppmRepPotus / 1000000;
					float DemHeight = (float)plotSizeY * ppmDemPotus / 1000000;

					float trumpY = zeroForAscendingY - RepHeight;
					float bidenY = zeroForDescendingY + DemHeight;

					// minus here, as +bonus plots upward, bu Windows GDI +y is downward:
					float trumpBonusY = zeroForBonusesY - (float)plotSizeY * bonusToTrump / 1000000;
					float bidenBonusY = zeroForBonusesY - (float)plotSizeY * bonusToBiden / 1000000;

					// clamp this slope on fat bars:
					
					float dXslope = barWidth * 0.1f;
					if(dXslope > 5.0f)
						dXslope = 5.0f; // arbitrary peeking at plot
					
					float midBarX1 = leftOfBar + dXslope; // misnomer now.
					float midBarX2 = leftOfBar + barWidth - dXslope; // second point.

					if(priorXreached != 0)
					{
						// connect the prior line segments to these new ones.
						
						// draw bonuses first
						gBmp.DrawLine(GreenBidenBonusLinePen, priorXreached, priorBidenBonusY, midBarX1, bidenBonusY); // pen, x1, y1, x2, y2
						gBmp.DrawLine(orangeTrumpBonusLinePen, priorXreached, priorTrumpBonusY, midBarX1, trumpBonusY); // pen, x1, y1, x2, y2

						// draw votes second on top
						gBmp.DrawLine(blueBidenLinePen, priorXreached, priorBidenY, midBarX1, bidenY); // pen, x1, y1, x2, y2
						gBmp.DrawLine(redTrumpLinePen, priorXreached, priorTrumpY, midBarX1, trumpY); // pen, x1, y1, x2, y2
					}
					
					{
						// always then draw a flat top/bottom line within grain bar.
						
						// draw bonuses first
						gBmp.DrawLine(GreenBidenBonusLinePen, midBarX1, bidenBonusY, midBarX2, bidenBonusY); // pen, x1, y1, x2, y2
						gBmp.DrawLine(orangeTrumpBonusLinePen, midBarX1, trumpBonusY, midBarX2, trumpBonusY); // pen, x1, y1, x2, y2

						// draw votes second on top
						gBmp.DrawLine(blueBidenLinePen, midBarX1, bidenY, midBarX2, bidenY); // pen, x1, y1, x2, y2
						gBmp.DrawLine(redTrumpLinePen, midBarX1, trumpY, midBarX2, trumpY); // pen, x1, y1, x2, y2
					}
					
					priorXreached = midBarX2;
					priorTrumpY = trumpY;
					priorBidenY = bidenY;
					priorTrumpBonusY = trumpBonusY;
					priorBidenBonusY = bidenBonusY;

					// Save this X to tie a bow on it for Pennsylvania
					
					// Special, to tie a bow on the results for Oakland County
					// Draw a special final line and legend to highlight big steal.

					// I am in too big a hurry to think clearly.
					// just do it in terms of the plot numbers.
					// Remember, Y is increasing downward:
					
					// These scalled-to-ppm bonuses are still +/- around ZERO here:
					
					// wrong to order by differece, but here require symmetry:
					// if(bonusToTrump < -XMarksTheSpotThreshold
					//   && bonusToBiden > XMarksTheSpotThreshold)
					if(bonusToBiden - bonusToTrump > 2   *XMarksTheSpotThreshold)
					{
						countTheBidenXMarksTheSpot ++;
						
						// The faster I run, the behinder I get!

						// go back to the CSV data, where 1M = 10% Full Scale.
						// Actually, here was the computation: 10% FS, or 20% FS:
						//int ppmTrumpBonus = ppmRepPotus - ppmRepOther;
						//int ppmBidenBonus = ppmDemPotus - ppmDemOther;
						//int scaleFactor = (do21gratsfspm10 ? 5 : 10);
						//int bonusToTrump = 500000 + scaleFactor * ppmTrumpBonus; // both plot up==positive
						//int bonusToBiden = 500000 + scaleFactor * ppmBidenBonus; // both plot up==positive

						// My head hurts. Reverse that computation:
						// Only 3K votes cannot be right!
						// Whoops, stop using integers here
						int ppmBonusToTrump = (int)((int.Parse(fields[7]) - 500000) / (double)scaleFactor);
						int ppmBonusToBiden = (int)((int.Parse(fields[8]) - 500000) / (double)scaleFactor);
						// still in scope: int grainBallots = int.Parse(fields[9]);
						
						// No it was not that, the numbers ARE really small.
						// Summing the 275 rows in Excel gives only 362,558 total votes
						// So 3393 votes is almost 1%.

						// Very Curious Negative, in Colorado, must study:
						// MARKING AN X ON [ElPaso] for shifting -1733 votes.
						// Hand running ballots=378234, BidenBonus=770135, TrumpBonus=395620,
						// I get a product here of 12,538,078,866, about 6 times the int.MaxValue.

						int netShiftForBidenVotes = (int)((long)(ppmBonusToBiden-ppmBonusToTrump) * (long)grainBallots / 1000000L);
						effectOfBidenBonusOnVotes += netShiftForBidenVotes;
						
						// give me a vertical to emphasize the two bonuses.
						// Actually, draw an X to mark the spot:
						// Fat pen is pretty to hold up, but covers the detail
						gBmp.DrawLine(blackXPen, midBarX1, bidenBonusY, midBarX2, trumpBonusY); // pen, x1, y1, x2, y2
						gBmp.DrawLine(blackXPen, midBarX2, bidenBonusY, midBarX1, trumpBonusY); // pen, x1, y1, x2, y2
						
						// mention where it happened
						// say("MARKING AN X ON [" + fields[10] + "] for shifting " + netShiftForBidenVotes + " votes.");
					}
					
					// just for balance, make white X's for such a "Trump Theft".
					// I have no counter or sum for these, visual plot mark only.

					if(bonusToTrump - bonusToBiden > 2   *XMarksTheSpotThreshold)
					{
						countTheTrumpXMarksTheSpot ++;
						
						int ppmBonusToTrump = (int)((int.Parse(fields[7]) - 500000) / (double)scaleFactor);
						int ppmBonusToBiden = (int)((int.Parse(fields[8]) - 500000) / (double)scaleFactor);
						int netShiftForTrumpVotes = (int)((long)(ppmBonusToTrump-ppmBonusToBiden) * (long)grainBallots / 1000000L);
						effectOfTrumpBonusOnVotes += netShiftForTrumpVotes;
						gBmp.DrawLine(whiteXPen, midBarX1, bidenBonusY, midBarX2, trumpBonusY); // pen, x1, y1, x2, y2
						gBmp.DrawLine(whiteXPen, midBarX2, bidenBonusY, midBarX1, trumpBonusY); // pen, x1, y1, x2, y2

						// say("MARKING WHITE X ON [" + fields[10] + "] for shifting " + netShiftForTrumpVotes + " votes.");
					}

					
					leftOfBar += barWidth;
				}
			}
			
			// draw the legends

			{
				// Draw most text in black
				Brush blackTextBrush = new SolidBrush(Color.Black);

				// Republican RED looks nice for this bold need:
				Brush conclusionRedTextBrush = new SolidBrush(Color.FromArgb(233, 20, 29));
				Brush flipsideWhiteTextBrush = new SolidBrush(Color.White);
				
				Font bigFont = new Font("Arial Black", 80);
				int bigFontHeight = (int)bigFont.GetHeight(gBmp);

				Font smallFont = new Font("Arial Black", 40);
				int smallFontHeight = (int)smallFont.GetHeight(gBmp);

				// Tweak this for the maximum width that fits
				Font conclusionFont = new Font("Arial Black", 52);
				int conclusionFontHeight = (int)conclusionFont.GetHeight(gBmp);

				Font unambiguousFont = new Font("Consolas", 35);
				int unambiguousFontHeight = (int)smallFont.GetHeight(gBmp);

				
				// above plot
				
				int yHeadine = nBorder / 2 - bigFontHeight / 2;
				string Headine = "Analyzing election data for " + LocationBeingStudiedClearly;
				gBmp.DrawString(Headine, bigFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(Headine, bigFont).Width) / 2, yHeadine);

				
				// out of 1000, all 50's fall on graticules. 25 / 75 are safe.


				// MAKING THE CONCLUSION MORE SPECTACULAR, AND RED!

				finalConclusion = "THE BLACK X'S " + insertRightSortedParenthesis + "MARK " + countTheBidenXMarksTheSpot + " BIGGEST SPOTS OF THEFT OVER " + (2*XPct) + "% WORTH " + effectOfBidenBonusOnVotes + " VOTES. SEE LOG FOR LOCALITY NAMES.";
				say("finalConclusion = " + finalConclusion);
				int YQEDLabel = nBorder + 25 * plotSizeY / 1000 - conclusionFont.Height / 2;
				string QEDLabel = finalConclusion;
				gBmp.DrawString(QEDLabel, conclusionFont, conclusionRedTextBrush, (imageSizeX-gBmp.MeasureString(QEDLabel, conclusionFont).Width) / 2, YQEDLabel);

				// Make room for this new white line next:

				flipsideConclusion = "FOR FAIRNESS, ANY WHITE X'S " + insertLeftSortedParenthesis + "MARK " + countTheTrumpXMarksTheSpot + " OPPOSITE BONUSES OVER " + (2*XPct) + "% FAVORING TRUMP WORTH " + effectOfTrumpBonusOnVotes + " VOTES.";
				say("flipsideConclusion = " + flipsideConclusion);
				int YDEQLabel = nBorder + 75 * plotSizeY / 1000 - conclusionFont.Height / 2;
				string DEQLabel = flipsideConclusion;
				gBmp.DrawString(DEQLabel, conclusionFont, flipsideWhiteTextBrush, (imageSizeX-gBmp.MeasureString(DEQLabel, conclusionFont).Width) / 2, YDEQLabel);
				
				int yMeaningLabel = nBorder + 125 * plotSizeY / 1000 - smallFontHeight / 2;
				string howDone = "INVALID";
				switch(howOther)
				{
					case 1:
						// howDone = "average SENATE & CONGRESS"; // too long
						howDone = "average SENATE+HOUSE";
						break;
					case 2:
						howDone = "SENATE";
						break;
					case 3:
						howDone = "CONGRESS";
						break;
				}

				string MeaningLabel = "Republicans are RED, Democrats are BLUE; Lines show % POTUS votes; Areas show % " + howDone + " votes.";
				gBmp.DrawString(MeaningLabel, smallFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(MeaningLabel, smallFont).Width) / 2, yMeaningLabel);

				
				int yFraudLabel = nBorder + 175 * plotSizeY / 1000 - smallFontHeight / 2;
				string FraudLabel = "ELECTION FRAUD CLUE WHENEVER RED AND/OR BLUE LINES SYSTEMATICALLY DO NOT TRACK THEIR AREA EDGE.";
				gBmp.DrawString(FraudLabel, smallFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(FraudLabel, smallFont).Width) / 2, yFraudLabel);

				
				string fullScale = (do21gratsfspm10? "Full scale = +/-10%": "Full scale = +/-5%");

				int YBidenLabel = nBorder + 875 * plotSizeY / 1000 - smallFontHeight / 2;
				string BidenLabel = "The GREEN line amplifies any % Bonus To BIDEN, above (or loss below) fat center graticule. " + fullScale;
				gBmp.DrawString(BidenLabel, smallFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(BidenLabel, smallFont).Width) / 2, YBidenLabel);

				
				int YTrumpLabel = nBorder + 925 * plotSizeY / 1000 - smallFontHeight / 2;
				string TrumpLabel = "The ORANGE line amplifies any % Bonus To TRUMP, above (or loss below) fat center graticule. " + fullScale;
				gBmp.DrawString(TrumpLabel, smallFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(TrumpLabel, smallFont).Width) / 2, YTrumpLabel);

				
				// 975 was my plan, but lands on BUCKS TRUMP MINUS
				int yOrderLabel = nBorder + 825 * plotSizeY / 1000 - smallFontHeight / 2;
				string AboveLabel = "Localities (\"" + GrainTag + "\") are plotted <-- left to right --> by " + orderingPrinciple + ".";
				gBmp.DrawString(AboveLabel, smallFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(AboveLabel, smallFont).Width) / 2, yOrderLabel);

				
				// below plot
				
				int yGitHubLabel = nBorder + 105 * plotSizeY / 100 - smallFontHeight / 2;
				string GitHubLabel = "Open Source: " + gitHubRepositoryShortened + " = " + gitHubRepositoryURL;
				gBmp.DrawString(GitHubLabel, unambiguousFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(GitHubLabel, unambiguousFont).Width) / 2, yGitHubLabel);

				
				int yVersionLabel = nBorder + 108 * plotSizeY / 100 - smallFontHeight / 2;
				string VersionLabel = DateStampPlot;
				gBmp.DrawString(VersionLabel, unambiguousFont, blackTextBrush, (imageSizeX-gBmp.MeasureString(VersionLabel, unambiguousFont).Width) / 2, yVersionLabel);

			}

			bmp.Save(pngFilePath, ImageFormat.Png); // can overwrite old png
		}
		
		
		
		// ==========================================
		// New input front-end for Pima County AZ
		// ==========================================

		
		// QUESTION COLON (?:...) is the non-capturing prefix:
		static Regex regexCRLFs = new Regex("(?:\r\n|\r|\n)", RegexOptions.Compiled);

		// QUESTION COLON (?:...) is the non-capturing prefix:
		// There are adjacent tabs, but they are empty fields.
		static Regex regexOneTab = new Regex("(?:\t)", RegexOptions.Compiled);

		static char [] caSpacesTabs = {' ', '\t'};
		
		static void InputDataFromTextFile()
		{
			// goal: call ponderInput(contest, choice, party, grain, votes);
			string contest = "";
			string choice = "";
			string party = "";
			string grain = "";
			string votes = "";
			
			string inputFilePath = Path.Combine(inputDir, inputTextFileName);

			string body = File.ReadAllText(inputFilePath);
			//say("body.Length = " + body.Length); // 27Mb
			
			string [] lines = regexCRLFs.Split(body);
			//say("lines.Length = " + lines.Length); // 155K
			
			//int minFields = int.MaxValue;
			//int maxFields = int.MinValue;
			int lineNo = 0;
			//int nTrimmed = 0;
			//int nPotus = 0;
			//int nSenus = 0;
			//int nRepus = 0;
			// string priorFieldZero = "";
			foreach(string line in lines)
			{
				lineNo ++;

				// no whitespace, trim not required:
				//string line2 = line.Trim(caSpacesTabs);
				//if(line2.Length != line.Length)
				//	nTrimmed ++;
				
				if(line.Length == 0) // any at eof?
					continue;
				
				string [] fields = regexOneTab.Split(line);
				
				// all lines have 39 fields
				//if(minFields > fields.Length)
				//{
				//	minFields = fields.Length;
				//	say("line " + lineNo + ": fields.Length = " + fields.Length);
				//}
				//if(maxFields < fields.Length)
				//{
				//	maxFields = fields.Length;
				//	say("line " + lineNo + ": fields.Length = " + fields.Length);
				//}
				
				// top row has column headers
				//if(lineNo == 1)
				//{
				//	for(int i = 0; i < fields.Length; i++)
				//	{
				//		say("fields[" + i + "] = " + fields[i]);
				//	}
				//}

				//fields[0] = ContestId
				//fields[1] = ContestExtId
				//fields[2] = ContestName
				contest = fields[2];
				//fields[3] = ContestPartyAffiliation
				//fields[4] = ContestVoteFor
				//fields[5] = ContestType
				//fields[6] = ContestOrder
				//fields[7] = PrecinctId
				//fields[8] = PrecinctExtId
				//fields[9] = PrecinctName
				grain = fields[9];
				//fields[10] = PrecinctOrder
				//fields[11] = PrecinctStatus
				//fields[12] = PrecinctRegistered
				//fields[13] = PrecinctTurnout
				//fields[14] = PrecinctTurnoutPerc
				//fields[15] = CandidateId
				//fields[16] = CandidateExtId
				//fields[17] = CandidateName
				choice = fields[17];
				//fields[18] = CandidateType
				//fields[19] = CandidateAffiliation
				party = fields[19];
				//fields[20] = CandidateOrder
				//fields[21] = Registered
				//fields[22] = Turnout
				//fields[23] = TurnoutPerc
				//fields[24] = Overvotes
				//fields[25] = Undervotes
				//fields[26] = Votes
				votes = fields[26];
				//fields[27] = Turnout_EARLY VOTE
				//fields[28] = Overvotes_EARLY VOTE
				//fields[29] = Undervotes_EARLY VOTE
				//fields[30] = Votes_EARLY VOTE
				//fields[31] = Turnout_ELECTION DAY
				//fields[32] = Overvotes_ELECTION DAY
				//fields[33] = Undervotes_ELECTION DAY
				//fields[34] = Votes_ELECTION DAY
				//fields[35] = Turnout_PROVISIONAL
				//fields[36] = Overvotes_PROVISIONAL
				//fields[37] = Undervotes_PROVISIONAL
				//fields[38] = Votes_PROVISIONAL

				// Left column is an ascending race number
				//if(priorFieldZero != fields[0])
				//{
				//	priorFieldZero = fields[0];
				//	say("Race " + priorFieldZero + ": " + fields[1]);
				//}
				
				//switch(fields[2])
				//{
				//	case "Presidential Electors":
				//		nPotus ++;
				//		break;
				//	case "US Senate-Term Expires  JANUARY 3, 2023":
				//		nSenus ++;
				//		break;
				//	case "US Rep Dist CD-1":
				//	case "US Rep Dist CD-2": // No CD-2 was seen, but...
				//	case "US Rep Dist CD-3":
				//	case "US Rep Dist CD-4":
				//	case "US Rep Dist CD-5":
				//	case "US Rep Dist CD-6":
				//	case "US Rep Dist CD-7":
				//	case "US Rep Dist CD-8":
				//	case "US Rep Dist CD-9":
				//		nRepus ++;
				//		break;
				//}

				
				// huh? System.FormatException: Input string was not in a correct format.
				// Oh, at lineNo 1!
				//say(lineNo + ": " + votes);
				if(lineNo > 1)
				{
					// Oh, looks like I omitted XML code that excludes minor races.
					// Do it now:
					switch(contest)
					{
						case contestPotus:
						case contestSenus1:
						case contestRepus1:
						case contestRepus2:
						case contestRepus3:
							ponderInput(contest, choice, party, grain, votes);
							break;
					}
					//e.g.,
					//Presidential Electors::TRUMP / PENCE::REP::0001 ACACIA::1475
					//Presidential Electors::BIDEN / HARRIS::DEM::0001 ACACIA::1770
				}
			}
			// say("nTrimmed = " + nTrimmed); // none, not required
			//say("nPotus = " + nPotus);
			//say("nSenus = " + nSenus);
			//say("nRepus = " + nRepus);
			// very curious, must explain: Oh, diff. qty candidates.
			//nPotus = 7440
			//nSenus = 15624
			//nRepus = 3237


		}

		// good name, reusing it for PIMA:
		
		static string inputXmlFileName = "PIMA County AZ ENR STANDARD XML.txt"; // sitting there
		
		
		// New inputter news a switch to omit all except:
		//contest: PRESIDENTIAL ELECTORS
		//contest: UNITED STATES SENATOR
		//contest: U.S. REPRESENTATIVE IN CONGRESS, DIST. 1
		//contest: U.S. REPRESENTATIVE IN CONGRESS, DIST. 2
		//contest: U.S. REPRESENTATIVE IN CONGRESS, DIST. 3

		static void inputPimaDetailXmlFile()
		{
			// probably steal many ideas from Clarity XML parser.
			
			// Goal: call ponderInput(contest, choice, party, grain, votes);
			
			string contest = "";
			string choice = "";
			string party = "";
			string grain = "";
			string votes = "0";
			
			XmlDocument doc = new XmlDocument();
			doc.Load(Path.Combine(inputDir,inputXmlFileName));

			XmlNode root = doc.DocumentElement;

			// I see Owner. It *IS* root, do not ask for it.
			// I see Election under that, I can select that:
			XmlNode OwElCoLiNode = root.SelectSingleNode("Election/ContestList");

			if(OwElCoLiNode == null)
				throw(new Exception("OwElCoLiNode == null"));
			// Within Election, I see hierarchy:
			// ContestList
			// Contest, attr title
			// Candidate, attr name, attr partyId
			// Votes, attr refPrecinctId
			// I can use numeric refPrecinctId as grain name.
			// Looking above, I learned:
			// <Party id="11" abbrv="DEM"
			// <Party id="12" abbrv="REP"

			XmlNodeList contestList = OwElCoLiNode.SelectNodes("Contest");
			//say("ConList.Count = " + ConList.Count); // 83
			
			foreach(XmlNode contestNode in contestList)
			{
				contest = contestNode.Attributes["title"].Value;
				
				switch(contest)
				{
					case contestStraight1:
						if(discardStraightPartyVotes)
							continue;
						break; // Else, Process this category

					case contestPotus:
						break; // Process this category

					case contestSenus1:
						break; // Process this category

					case contestRepus1:
					case contestRepus2:
					case contestRepus3:
						break; // Process this category

					default:
						continue; // discard all others
				}

				//say("contest: " + contest);

				XmlNodeList choiceList = contestNode.SelectNodes("Candidate");
				//say("choiceList.Count = " + choiceList.Count); // 83

				foreach(XmlNode choiceNode in choiceList)
				{
					choice = choiceNode.Attributes["name"].Value;
					//say("choice: " + choice);

					party = choiceNode.Attributes["partyId"].Value;
					switch(party)
					{
						case "11":
							party = "DEM";
							break;
						case "12":
							party = "REP";
							break;
						default:
							party = "ETC";
							break;
					}
					//say("party: " + party);

					XmlNodeList votesList = choiceNode.SelectNodes("Votes");
					//say("votesList.Count = " + votesList.Count); // 83
					
					foreach(XmlNode votesNode in votesList)
					{
						votes = votesNode.InnerText;
						grain = votesNode.Attributes["refPrecinctId"].Value;
						
						ponderInput(contest, choice, party, grain, votes);
					}
					
				}
			}
			say("Pima done");
		}

	}
}