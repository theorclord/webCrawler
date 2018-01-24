using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpCrawler
{
  public enum choice
  {
    open,close,can,not
  }
  public class KeyholderEntry : IComparable<KeyholderEntry>
  {
    public string Name { get; set; }
    public int numberOfHours { get; set; }
    // status on day
    // format date+int, can,not,open,close
    public Dictionary<string, choice> answer { get; set; }
    public int CompareTo(KeyholderEntry other)
    {
      return other.numberOfHours.CompareTo(numberOfHours);
    }
  }
}
