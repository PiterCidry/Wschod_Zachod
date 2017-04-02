using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml;
using Newtonsoft.Json;


namespace Wschod_Zachod
{
    class Program
    {
        static async Task<string>pobieranie(string adres)
        {
            HttpClient http = new HttpClient();
            try
            {
                HttpResponseMessage response = await http.GetAsync("https://maps.googleapis.com/maps/api/geocode/xml?address="+adres+"&Key=AIzaSyAB9ahVJHhWjhN2H8tqTJ8RkVY4azhyuXo");
                response.EnsureSuccessStatusCode();
                string address = await response.Content.ReadAsStringAsync();
                http.Dispose();
                return address;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException caught!\nMessage: {0}", e.Message);
                http.Dispose();
                return e.Message;
            }
        }
        static void zapiszXML(string nPliku, string wejscie)
        {
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(wejscie);
                xmlDoc.Save(nPliku);
            }
            catch(Exception e)
            {
                Console.WriteLine("\nException caught!\nMessage: {0}", e.Message);
            }
        }
        static string odczytajDlugosc(string nPliku)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(nPliku);
                XmlNode dlug = xmlDoc.SelectSingleNode("/GeocodeResponse/result/geometry/location/lng");
                string dlugosc = dlug.InnerText;
                return dlugosc;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException caught!\nMessage: {0}", e.Message);
                return "0";
            }
        }
        static string odczytajSzerokosc(string nPliku)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(nPliku);
                XmlNode szer = xmlDoc.SelectSingleNode("/GeocodeResponse/result/geometry/location/lat");
                string szerokosc = szer.InnerText;
                return szerokosc;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException caught!\nMessage: {0}", e.Message);
                return "0";
            }
        }
        static async Task<string>slonce(string dlugosc, string szerokosc)
        {
            HttpClient http = new HttpClient();
            try
            {
                HttpResponseMessage response = await http.GetAsync("http://api.sunrise-sunset.org/json?lat="+szerokosc+"&lng="+dlugosc);
                response.EnsureSuccessStatusCode();
                string slonce = await response.Content.ReadAsStringAsync();
                http.Dispose();
                return slonce;
            }
            catch(HttpRequestException e)
            {
                Console.WriteLine("\nException caught!\nMessage: {0}", e.Message);
                http.Dispose();
                return e.Message;
            }
        }
        static XmlDocument xmlParse(string json)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc = JsonConvert.DeserializeXmlNode(json, "wschodSlonca");
                return doc;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException caught!\nMessage: {0}", e.Message);
                return null;
            }
        }
        static string[] odczytajWschZachDldnia(string nPliku)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(nPliku);
                XmlNode wsch = xmlDoc.SelectSingleNode("/wschodSlonca/results/sunrise");
                XmlNode zach = xmlDoc.SelectSingleNode("/wschodSlonca/results/sunset");
                XmlNode dldn = xmlDoc.SelectSingleNode("/wschodSlonca/results/day_length");
                string[] wartosci = new string[3];
                wartosci[0] = wsch.InnerText;
                wartosci[1] = zach.InnerText;
                wartosci[2] = dldn.InnerText;
                return wartosci;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException caught!\nMessage: {0}", e.Message);
                return new string[0];
            }
        }
        static void Main(string[] args)
        {
            Console.Write("Podaj miasto: ");
            string miasto = Console.ReadLine();
            Console.Write("Podaj ulice: ");
            string ulica = Console.ReadLine();
            Console.Write("Podaj nr domu: ");
            string nrdomu = Console.ReadLine();
            Console.Write("Podaj kraj: ");
            string kraj = Console.ReadLine();
            string uri = (miasto+"+"+ulica+"+"+nrdomu+"+"+kraj);
            string adres = pobieranie(uri).Result;
            //Console.WriteLine(adres);
            zapiszXML("adres.xml", adres);
            string dlugosc = odczytajDlugosc("adres.xml"); 
            string szerokosc = odczytajSzerokosc("adres.xml");
            //Console.WriteLine("Dlugosc: " +dlugosc + "\tSzerokosc: " + szerokosc);
            string wschzach = slonce(dlugosc, szerokosc).Result;
            //Console.WriteLine(wschzach);
            XmlDocument wschod = xmlParse(wschzach);
            zapiszXML("wschod.xml", wschod.OuterXml);
            string[] wartosci = new string[3];
            wartosci = odczytajWschZachDldnia("wschod.xml");
            Console.WriteLine("Wschod slonca: "+wartosci[0]+"\nZachod slonca: "+wartosci[1]+"\nDlugosc dnia: "+wartosci[2]);
            Console.ReadKey();
        }
    }
}
