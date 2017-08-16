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
      // A list containing the names of the keyholders as displayed on the wiki
      List<string> listOfKeyholders = new List<string>();
      listOfKeyholders.Add("Adam");
      listOfKeyholders.Add("Andresen");
      listOfKeyholders.Add("Anne");
      listOfKeyholders.Add("Anno");
      listOfKeyholders.Add("arinbjorn");
      listOfKeyholders.Add("Bjarke");
      listOfKeyholders.Add("Bj�rn");
      listOfKeyholders.Add("Bundgaard");
      listOfKeyholders.Add("Caro");
      listOfKeyholders.Add("dusk");
      listOfKeyholders.Add("Dalfoss");
      listOfKeyholders.Add("Freja");
      listOfKeyholders.Add("jacob");
      listOfKeyholders.Add("Kasper");
      listOfKeyholders.Add("Marius");
      listOfKeyholders.Add("Mikael");
      listOfKeyholders.Add("Mimi");
      listOfKeyholders.Add("Nano");
      listOfKeyholders.Add("Natasha");
      listOfKeyholders.Add("Patrick");
      listOfKeyholders.Add("Polle");
      listOfKeyholders.Add("Pyrus");
      listOfKeyholders.Add("Rune");
      listOfKeyholders.Add("Sebastian");
      listOfKeyholders.Add("Sofie");
      listOfKeyholders.Add("Sven");
      listOfKeyholders.Add("Svenne");
      listOfKeyholders.Add("Theis");

      //List Used to create the headers for the csv file
      List<string> listOfDates = new List<string>();
      // add first element
      listOfDates.Add("Navne");

      // Dictionary containing the keyholder responses
      Dictionary<string, List<string>> keyHolderResp = new Dictionary<string, List<string>>();

      bool addDates = true;
      // go through each keyholder to get their availability.
      foreach (string keyHolderName in listOfKeyholders)
      {
        WebClient web = new WebClient();
        WebRequest request = WebRequest.Create("http://www.cafeen.org/internt/interntwiki/index.php/Drift/" + keyHolderName);
        request.Credentials = new System.Net.NetworkCredential("cafe", "Aristocats");
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        // Display the status.
        Console.WriteLine(response.StatusDescription);


        // Get the stream containing content returned by the server.
        Stream dataStream = response.GetResponseStream();
        // Open the stream using a StreamReader for easy access.

        Encoding iso = Encoding.GetEncoding("ISO-8859-1");
        Encoding utf8 = Encoding.UTF8;
        //byte[] utfBytes = utf8.GetBytes(Message);
        //byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
        //string msg = iso.GetString(isoBytes);

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
        bool firstRow = true;
        foreach (HtmlNode table in tableDates)
        {
          if (firstRow)
          {
            firstRow = false;
            continue;
          }
          HtmlNodeCollection rows = table.SelectNodes("tr");
          foreach (HtmlNode row in rows)
          {
            HtmlNodeCollection columns = row.SelectNodes("td");
            keyHolderResp[keyHolderName].Add(columns[4].InnerText);
            // add the dates as headers
            if (addDates)
            {
              listOfDates.Add(columns[0].InnerText);
            }
          }
        }
        System.IO.File.WriteAllText(@"C:\WriteInnerText.txt", innerText);

        addDates = false;
      }

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
      Console.ReadLine();

      /* ----------------------------------------------------------
                        Expanded CSV file creation
         ----------------------------------------------------------*/

      // Create final file looking as schema previously used
      // The file needs two string lines for each member
      // The file needs columns equal to the dates plus the user name
      //string[,] finalSchemaGrid = new string[listOfDates.Count,listOfKeyholders.Count*2];

      //// Out loop, the headers 
      //for(int i = 0; i < listOfDates.Count; i++)
      //{
      //  finalSchemaGrid[i, 0] = listOfDates[i];
      //  // Loop of keyholderNames
      //  for(int j = 1; j < listOfKeyholders.Count; j++)
      //  {
      //    finalSchemaGrid[0, (j*2)-1] = listOfKeyholders[j];
      //    //Loop of responses
      //    for(int k = 0; k < keyHolderResp[listOfKeyholders[j]].Count; k++)
      //    {
      //      switch (keyHolderResp[listOfKeyholders[j]][k].ToLower())
      //      {
      //        case "kan":
      //          finalSchemaGrid[k+1, (j*2)-1] = "x";
      //          finalSchemaGrid[k+1, (j*2)] = "x";
      //          break;
      //        case "ikke":
      //          finalSchemaGrid[k + 1, (j * 2) - 1] = "";
      //          finalSchemaGrid[k + 1, (j * 2)] = "";
      //          break;
      //        case "lukke":
      //          finalSchemaGrid[k + 1, (j * 2) - 1] = "";
      //          finalSchemaGrid[k + 1, (j * 2)] = "x";
      //          break;
      //        case "�bne":
      //          finalSchemaGrid[k + 1, (j * 2) - 1] = "x";
      //          finalSchemaGrid[k + 1, (j * 2)] = "";
      //          break;
      //        default:
      //          finalSchemaGrid[k + 1, j] = keyHolderResp[listOfKeyholders[j]][k];
      //          break;
      //      }
      //    }
      //  }
      //}

      //StringBuilder finalSchemaStringCSV = new StringBuilder();
      //for(int i = 0; i < finalSchemaGrid.GetLength(0); i++)
      //{
      //  for(int j = 0; j < finalSchemaGrid.GetLength(1); j++)
      //  {
      //    finalSchemaStringCSV.Append(finalSchemaGrid[i, j]);
      //    Console.Write(finalSchemaGrid[i, j]);
      //    if (j != finalSchemaGrid.GetLength(1) - 1)
      //    {
      //      finalSchemaStringCSV.Append(",");
      //      Console.Write(",");
      //    }
      //  }
      //  finalSchemaStringCSV.Append("\n");
      //  Console.WriteLine();
      //}
      //System.IO.File.WriteAllText(@"C:\FinalCsvTrans.txt", finalSchemaStringCSV.ToString());
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
              case "�bne":
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

      System.IO.File.WriteAllText(@"C:\FinalCsvTrans.txt", finalTransCSV.ToString(),Encoding.Default);
      Console.ReadLine();
      /*
       END OF PROGRAM
       */
    }
  }
}
