using System;
using System.Threading;

namespace Airline_app
{

    public delegate void priceCutEvent(Int32 pr);
    class Program
    {
        static void Main(string[] args)
        {

            Airline airline = new Airline(); 
            Thread farmer = new Thread(new ThreadStart(airline.farmerFunc));
            farmer.Start();         // Start one farmer thread
            TravelAgency travelAgency = new TravelAgency();
            Airline.priceCut += new priceCutEvent(travelAgency.chickenOnSale);
            Thread[] travelAgencies = new Thread[5];
            for (int i = 0; i < 5; i++)
            {   // Start N retailer threads
                travelAgencies[i] = new Thread(new ThreadStart(travelAgency.retailerFunc));
                travelAgencies[i].Name = (i + 1).ToString();
                travelAgencies[i].Start();
            }

        }
    }

    public class Airline //Airline
    {
        static Random rng = new Random();
        public static event priceCutEvent priceCut; // Define event
        private static Int32 chickenPrice = 70;
        


        public Int32 getPrice() {
            
            return chickenPrice;

        }

        public static void changePrice(Int32 price)
        {
            
            if (price < chickenPrice)
            { // a price cut 
                if (priceCut != null)   // there is at least a subscriber
                    priceCut(price);    // emit event to subscribers
            }
            chickenPrice = price;
        }

        public void farmerFunc()
        {
            for (Int32 i = 0; i < 20; i++) //price cuts stops after 20
            {
                Thread.Sleep(500);
                // Take the order from the queue of the orders;
                // Decide the price based on the orders

                Int32 p = rng.Next(50, 200); // Generate a random price
               // Console.WriteLine("New Price is {0}", p);
                Airline.changePrice(p);
            }
        }
    }

    public class TravelAgency //Travel Agency
    {

        public void retailerFunc()
        {   //for starting thread
            Airline airline = new Airline();
            for (Int32 i = 0; i < 20; i++) //iterate per each thread so store2 will go n number of times
            {
                Thread.Sleep(500);
                Int32 p = airline.getPrice();
                Console.WriteLine("Travel Agency{0} has everyday low price: ${1} each", Thread.CurrentThread.Name, p);
            }
        }

        public void chickenOnSale(Int32 p)
        {  // Event handler
            //create order object to send all values through a buffer for the airline to process
            Console.WriteLine("Travel Agency{0} tickets are on sale: as low as ${1} each", Thread.CurrentThread.Name, p);
        }
    }

    public class Order
    {
        private string senderId;
        private int cardNo;
        private int amount;
        private double unitPrice;

        public void setSender(string senderId)
        {
            this.senderId = senderId;
        }

        public void setCardNumber(int num)
        {
            this.cardNo = num;
        }

        public void setAmount(int num)
        {
            this.amount = num;
        }

        public void setUnitPrice(int price)
        {
            this.unitPrice = price;
        }
    }

    public class MultiCellBuffer
    {

    }





}

