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
      listOfKeyholders.Add("Alex");
      listOfKeyholders.Add("arinbjorn");
      listOfKeyholders.Add("Bjarke");
      listOfKeyholders.Add("Bjørn");
      listOfKeyholders.Add("Bundgaard");
      listOfKeyholders.Add("camilla");
      listOfKeyholders.Add("Caro");
      listOfKeyholders.Add("dusk");
      listOfKeyholders.Add("Freja");
      listOfKeyholders.Add("jacob");
      listOfKeyholders.Add("Jenny");
      listOfKeyholders.Add("John");
      listOfKeyholders.Add("Kasper");
      listOfKeyholders.Add("Marius");
      listOfKeyholders.Add("Mimi");
      listOfKeyholders.Add("Nano");
      listOfKeyholders.Add("Natasha");
      listOfKeyholders.Add("Nicklas");
      listOfKeyholders.Add("Paw");
      listOfKeyholders.Add("Patrick");
      listOfKeyholders.Add("Pyrus");
      listOfKeyholders.Add("Rune");
      listOfKeyholders.Add("Simba");
      listOfKeyholders.Add("Sofie");
      listOfKeyholders.Add("Sven");
      listOfKeyholders.Add("Svenne");
      listOfKeyholders.Add("Søren");

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
        request.Credentials = new System.Net.NetworkCredential("cafe", "Rammstein");
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        // Display the status.
        Console.WriteLine(response.StatusDescription);


        // Get the stream containing content returned by the server.
        Stream dataStream = response.GetResponseStream();
        // Open the stream using a StreamReader for easy access.
        StreamReader reader = new StreamReader(dataStream);
        // Read the content.
        string responseFromServer = reader.ReadToEnd();
        // Display the content.
        //Console.WriteLine(responseFromServer);
        // Cleanup the streams and the response.
        reader.Close();
        dataStream.Close();
        response.Close();
        System.IO.File.WriteAllText(@"C:\WriteText.txt", responseFromServer);

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
            //Console.WriteLine(columns[0].InnerText + " " + columns[4].InnerText);
            keyHolderResp[keyHolderName].Add(columns[4].InnerText);
            // add the dates as headers
            if (addDates)
            {
              listOfDates.Add(columns[0].InnerText);
            }
          }
        }
        //XmlNode pageContent = xmlDoc.SelectSingleNode("html/body/table[@id='wikimid']/tr/td[@id='wikibody']/div[@id='wikitext']");
        //string[] splitString = responseFromServer.Split(new string[] { "<div id='wikitext'>" },StringSplitOptions.None);
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
      System.IO.File.WriteAllText(@"C:\FinalSchema.txt", finalText);
      Console.WriteLine(csvFileLine);
      Console.ReadLine();
    }
   }
}
