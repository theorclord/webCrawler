using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using HtmlAgilityPack;

namespace CSharpCrawler
{
  class Program
  {
    static void Main(string[] args)
    {
      string userName = "";
      string filePath = @"";
      string pass = "";
      if (args.Length == 0)
      {
        Console.WriteLine("Enter password and user name for cafeen.org");
        Console.Write("Enter user name: ");
        userName = Console.ReadLine();
        Console.Write("Enter password: ");
        pass = Console.ReadLine();

        Console.WriteLine();
        Console.Write("Enter file path for keyholders: ");
        filePath = Console.ReadLine();
      } else
      {
        if(args.Length == 3)
        {
          userName = args[0];
          pass = args[1];
          filePath = args[2];
        } else
        {
          Console.WriteLine("Wrong number of arguments");
          return;
        }
      }
      string[] keyholders = File.ReadAllLines(filePath);
      // A list containing the names of the keyholders as displayed on the wiki
      List<string> listOfKeyholders = new List<string>(keyholders);

      //List Used to create the headers for the csv file
      List<string> listOfDates = new List<string>();
      // add first element
      listOfDates.Add("Navne");

      // Dictionary containing the keyholder responses
      Dictionary<string, List<string>> keyHolderResp = new Dictionary<string, List<string>>();

      #region HTML load
      bool addDates = true;
      // go through each keyholder to get their availability.
      foreach (string keyHolderName in listOfKeyholders)
      {
        //WebClient web = new WebClient();
        
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.cafeen.org/internt/interntwiki/index.php/Drift/" + keyHolderName);
        // if the request fails:
        // kigge på stackoverflow, hvis den IT-ansvarlige ikke holder certifikatet opdateret

        //request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
        //request.Headers.Set(const_AcceptLanguageHeaderName, const_AcceptLanguageHeader);
        //request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";
        //request.CookieContainer = new CookieContainer();
        //request.KeepAlive = true;
        //request.Timeout = 1000;

        request.Credentials = new System.Net.NetworkCredential(userName, pass);
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        // Display the status.
        Console.WriteLine(response.StatusDescription);


        // Get the stream containing content returned by the server.
        Stream dataStream = response.GetResponseStream();
        // Open the stream using a StreamReader for easy access.

        Encoding iso = Encoding.GetEncoding("ISO-8859-1");
        Encoding utf8 = Encoding.UTF8;

        StreamReader reader = new StreamReader(dataStream, iso);
        // Read the content.
        string responseFromServer = reader.ReadToEnd();
        // Display the content.
        //Console.WriteLine(responseFromServer);
        // Cleanup the streams and the response.
        reader.Close();
        dataStream.Close();
        response.Close();
        System.IO.File.WriteAllText(@"C:\WriteText.txt", responseFromServer,Encoding.Default);

        // The HtmlWeb class is a utility class to get the HTML over HTTP
        HtmlWeb htmlWeb = new HtmlWeb();

        // Creates an HtmlDocument object from an URL
        HtmlAgilityPack.HtmlDocument document = new HtmlDocument();
        document.Load(@"C:\WriteText.txt");

        // Targets a specific node
        HtmlNode wikiText = document.GetElementbyId("wikitext");
        string innerText = wikiText.InnerHtml;

        HtmlNodeCollection tableDates = wikiText.SelectNodes("table");

        keyHolderResp[keyHolderName] = new List<string>();

        // skip first table, as it only contains headers
        // removed as the format has changed
        // bool firstRow = true;
        foreach (HtmlNode table in tableDates)
        {
          //if (firstRow)
          //{
          //  firstRow = false;
          //  continue;
          //}
          HtmlNodeCollection rows = table.SelectNodes("tr");
          foreach (HtmlNode row in rows)
          {
            HtmlNodeCollection columns = row.SelectNodes("td");
            if(columns == null)
            {
              //the top column contains the th
              continue;
            }
            keyHolderResp[keyHolderName].Add(columns[4].InnerText);
            // add the dates as headers
            if (addDates)
            {
              listOfDates.Add(columns[0].InnerText);
            }
          }
        }
        //System.IO.File.WriteAllText(@"C:\WriteInnerText.txt", innerText);

        addDates = false;
      }
      // clean up
      File.Delete(@"C:\WriteText.txt");
      #endregion
      /* ----------------------------------------------------------
                        CSV file creation
         ----------------------------------------------------------*/
      List<string> listOfCsvLines = new List<string>();
      string csvFileLine = "";
      for(int i = 0; i < listOfDates.Count; i++)
      {
        if (i == 0)
        {
          csvFileLine = listOfDates[i];
        } else
        {
          csvFileLine += "," + listOfDates[i];
        }
      }
      listOfCsvLines.Add(csvFileLine);
      string csvKeyRspline = "";
      foreach(KeyValuePair<string,List<string>> keyResp in keyHolderResp)
      {
        csvKeyRspline = keyResp.Key;
        for (int i = 0; i< keyResp.Value.Count; i++)
        {
          csvKeyRspline += "," + keyResp.Value[i];
        }
        listOfCsvLines.Add(csvKeyRspline);
      }
      
      string finalText = "";
      for(int i = 0; i < listOfCsvLines.Count; i++)
      {
        if(i != (listOfCsvLines.Count - 1))
        {
          finalText += listOfCsvLines[i] +"\n";
        } else
        {
          finalText += listOfCsvLines[i];
        }
      }
      System.IO.File.WriteAllText(@"C:\FinalSchema.txt", finalText, Encoding.Default);
      Console.WriteLine(csvFileLine);
      //Console.ReadLine();
      
      /* ----------------------------------------------------------
                        Expanded CSV file creation From initial file
         ----------------------------------------------------------*/
      Console.WriteLine("Writing finalFile");
      List<List<string>> finalList = new List<List<string>>();
      StreamReader sr = new StreamReader(File.OpenRead(@"C:\FinalSchema.txt"), Encoding.Default);

      bool firstLine = true;
      while(sr.Peek() >0)
      {
        string[] line = sr.ReadLine().Split(',');
        if (firstLine)
        {
          finalList.Add(new List<string>(line));
          firstLine = false;
          continue;
        }
        List<string> keyRespLine1 = new List<string>();
        List<string> keyRespLine2 = new List<string>();
        for(int i = 0; i < line.Length; i++)
        {
          if(i == 0)
          {
            keyRespLine1.Add(line[i]);
            keyRespLine2.Add("");
          } else
          {
            switch (line[i].ToLower())
            {
              case "kan":
                keyRespLine1.Add("x");
                keyRespLine2.Add("x");
                break;
              case "ikke":
                keyRespLine1.Add("");
                keyRespLine2.Add("");
                break;
              case "lukke":
                keyRespLine1.Add("");
                keyRespLine2.Add("x");
                break;
              case "åbne":
                keyRespLine1.Add("x");
                keyRespLine2.Add("");
                break;
              default:
                keyRespLine1.Add(line[i].ToLower());
                keyRespLine2.Add("");
                break;
            }
          }
        }
        finalList.Add(keyRespLine1);
        finalList.Add(keyRespLine2);
      }

      StringBuilder finalTransCSV = new StringBuilder();
      for(int i = 0; i< finalList.Count; i++)
      {
        for(int j=0; j < finalList[i].Count; j++)
        {
          if(j!= 0)
          {
            finalTransCSV.Append(",");
          }
          finalTransCSV.Append(finalList[i][j]);
        }
        finalTransCSV.Append("\n");
      }
      sr.Close();
      System.IO.File.WriteAllText(@"C:\FinalCsvTrans.txt", finalTransCSV.ToString(),Encoding.Default);
      //Console.ReadLine();


      // greedy schedule assignment

      // load file and create priority queue

      string[] csvLines = File.ReadAllLines(@"C:\FinalCsvTrans.txt");
      // list for filling priorityqueue
      List<KeyholderEntry> keyHoldersList = new List<KeyholderEntry>();
      // list for easy holder update.
      Dictionary<string, KeyholderEntry> holderList = new Dictionary<string, KeyholderEntry>();
      for(int i = 1; i<csvLines.Length; i += 2)
      {
        KeyholderEntry keyEntry = new KeyholderEntry() { Name = csvLines[i].Split(',')[0], numberOfHours = 0, answer = new Dictionary<string, choice>() };
        keyHoldersList.Add(keyEntry);
        holderList.Add(csvLines[i].Split(',')[0], keyEntry);
      }

      List<string> linesWithHours = new List<string>();
      linesWithHours.Add(csvLines[0]);
      for (int j = 1; j < csvLines[0].Split(',').Length; j++)
      {
        for (int i = 1; i < csvLines.Length; i += 2)
        {
          string[] dataLine1 = csvLines[i].Split(',');
          string[] dataLine2 = csvLines[i+1].Split(',');
          string dayCheckOpen = dataLine1[j];
          string dayCheckClose = dataLine2[j];

          string KeyName = dataLine1[0];
          if (holderList.ContainsKey(KeyName))
          {
            if(dayCheckOpen.Trim().ToLower() == "x" && dayCheckClose.Trim().ToLower() == "x")
            {
              holderList[KeyName].answer.Add(csvLines[0].Split(',')[j], choice.can);
            } else if(dayCheckOpen.Trim().ToLower() == "x")
            {
              holderList[KeyName].answer.Add(csvLines[0].Split(',')[j], choice.open);
            } else if(dayCheckClose.Trim().ToLower() == "x")
            {
              holderList[KeyName].answer.Add(csvLines[0].Split(',')[j], choice.close);
            }
            else
            {
              holderList[KeyName].answer.Add(csvLines[0].Split(',')[j], choice.not);
            }
          }
          else
          {
            Console.WriteLine("Error in assigning shifts");
            return;
          }
        }
      }

      string endLine1 = "";
      string endLine2 = "";
      for(int i = 1; i< csvLines[0].Split(',').Length; i++)
      {
        PriorityQueueMin<KeyholderEntry> keyholderQueue = new PriorityQueueMin<KeyholderEntry>();
        keyholderQueue.insertRange(keyHoldersList);
        string date = csvLines[0].Split(',')[i];
        bool openChosen = false;
        bool closeChosen = false;

        string openerKeyName = "";
        string closerKeyName = "";
        while(keyholderQueue.Count != 0 && (!openChosen || !closeChosen))
        {
          KeyholderEntry key = keyholderQueue.DelMin();
          if (openerKeyName != key.Name && closerKeyName != key.Name)
          {
            if (!openChosen)
            {
              if (key.answer[date] == choice.can || key.answer[date] == choice.open)
              {
                openChosen = true;
                endLine1 += "," + key.Name;
                openerKeyName = key.Name;
                DateTime dateDay = new DateTime(int.Parse(date.Substring(5, 4)), int.Parse(date.Substring(2, 2)), int.Parse(date.Substring(0, 2)));
                switch (dateDay.DayOfWeek)
                {
                  case DayOfWeek.Sunday:
                    break;
                  case DayOfWeek.Monday:
                    key.numberOfHours += 5;
                    break;
                  case DayOfWeek.Tuesday:
                    key.numberOfHours += 4;
                    break;
                  case DayOfWeek.Wednesday:
                    key.numberOfHours += 4;
                    break;
                  case DayOfWeek.Thursday:
                    key.numberOfHours += 5;
                    break;
                  case DayOfWeek.Friday:
                    key.numberOfHours += 8;
                    break;
                  case DayOfWeek.Saturday:
                    break;
                }
              }
            }
            if (!closeChosen)
            {
              if (key.answer[date] == choice.can || key.answer[date] == choice.close)
              {
                closeChosen = true;
                endLine2 += "," + key.Name;
                closerKeyName = key.Name;
                DateTime dateDay = new DateTime(int.Parse(date.Substring(5, 4)), int.Parse(date.Substring(2, 2)), int.Parse(date.Substring(0, 2)));
                switch (dateDay.DayOfWeek)
                {
                  case DayOfWeek.Sunday:
                    break;
                  case DayOfWeek.Monday:
                    key.numberOfHours += 5;
                    break;
                  case DayOfWeek.Tuesday:
                    key.numberOfHours += 4;
                    break;
                  case DayOfWeek.Wednesday:
                    key.numberOfHours += 4;
                    break;
                  case DayOfWeek.Thursday:
                    key.numberOfHours += 5;
                    break;
                  case DayOfWeek.Friday:
                    key.numberOfHours += 8;
                    break;
                  case DayOfWeek.Saturday:
                    break;
                }
              }
            }
          }
        }
        if (!openChosen)
        {
          endLine1 += ",";
          endLine2 += ",";
        }
      }
      //string[] endLines = new string[] { "",endLine1, endLine2 };
      Console.WriteLine("Algorithm Done");
      //File.AppendAllLines(@"C:\FinalCsvTrans.txt", endLines, Encoding.Default);

      List<string> listWithAlgorithm = new List<string>();
      listWithAlgorithm.Add(csvLines[0]);
      for(int i = 1; i< csvLines.Length; i+=2)
      {
        if(holderList.ContainsKey(csvLines[i].Split(',')[0]))
        {
          csvLines[i + 1] = "" + holderList[csvLines[i].Split(',')[0]].numberOfHours + csvLines[i + 1];
        }
        listWithAlgorithm.Add(csvLines[i]);
        listWithAlgorithm.Add(csvLines[i+1]);
      }
      listWithAlgorithm.Add(endLine1);
      listWithAlgorithm.Add(endLine2);
      File.WriteAllLines(@"C:\FinalCsvTransWithHours.txt", listWithAlgorithm.ToArray(), Encoding.Default);

      File.Delete(@"C:\FinalCsvTrans.txt");
      File.Delete(@"C:\FinalSchema.txt");
      /*
       END OF PROGRAM
       */
    }
  }
}
