using System;
using System.Threading;

namespace Airline_app
{
    public delegate void priceCutEvent(Int32 pr);
    class Program
    {
       

        static void Main(string[] args)
        {
           
            MultiCellBuffer multiCellBuffer = new MultiCellBuffer();
            Airline airline = new Airline(multiCellBuffer); 

            Thread farmer = new Thread(new ThreadStart(airline.airlineFunc));

            farmer.Start();         // Start one farmer thread

            TravelAgency travelAgency = new TravelAgency(multiCellBuffer);

            Airline.priceCut += new priceCutEvent(travelAgency.ticketsOnSale);

            Thread[] travelAgencies = new Thread[5];
            for (int i = 0; i < 5; i++)
            {   // Start N retailer threads
                travelAgencies[i] = new Thread(new ThreadStart(travelAgency.travelAgencyFunc));
                travelAgencies[i].Name = (i + 1).ToString();
                travelAgencies[i].Start();
            }
            

        }
    }

    public class Airline //Consumer
    {
        static Random rng = new Random();
        public static event priceCutEvent priceCut; // Define event
        private static Int32 chickenPrice = 70;
        private MultiCellBuffer multiCellBuffer;

        public Airline(MultiCellBuffer multiCellBuffer)
        {
            this.multiCellBuffer = multiCellBuffer;
        }

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

        public void airlineFunc()
        {
            for (Int32 i = 0; i < 20; i++) //price cuts stops after 20
            {
                Thread.Sleep(500);
                // Take the order from the queue of the orders;
                // Decide the price based on the orders

                Int32 p = rng.Next(50, 200); // Generate a random price
               // Console.WriteLine("New Price is {0}", p);
                Airline.changePrice(p);

                lock (multiCellBuffer)
                {
                    if (multiCellBuffer.order != null)
                    {
                        if(multiCellBuffer.order.getAmount() != 0)
                        {
                            Console.WriteLine("Airline is Processing Order for Travel Agency {0} ------------------------------", multiCellBuffer.order.getSender());
                        }
                    
                        multiCellBuffer.getOneCell();
                    
                    }
                }

            }
        }
    }

    public class TravelAgency //Producer
    {
        int onSaleFlag = 0;
        private MultiCellBuffer multiCellBuffer;
        static Random rng = new Random();
        public TravelAgency(MultiCellBuffer multiCellBuffer)
        {
            this.multiCellBuffer = multiCellBuffer;
        }

        public void travelAgencyFunc()
        {
            //object box = onSaleFlag;   
            Order order = new Order();
            //for starting thread
            Airline airline = new Airline(multiCellBuffer);
            for (Int32 i = 0; i < 20; i++) //iterate per each thread so store2 will go n number of times
            {
                Thread.Sleep(500);
                Int32 p = airline.getPrice();
               
                Console.WriteLine("Travel Agency{0} has everyday low price: ${1} each", Thread.CurrentThread.Name, p);

                lock (multiCellBuffer)
                {
                    if (onSaleFlag == 1)
                    {

                        Console.WriteLine("--------------------------------------- Travel Agency {0} is Making an order for price {1}", Thread.CurrentThread.Name, p);
                        order.setAmount(2);
                        order.setCardNumber(rng.Next(4000, 7000));
                        order.setSender(Thread.CurrentThread.Name);
                        multiCellBuffer.setOneCell(order);
                        onSaleFlag = 0;

                    }
                }  

            }

        }

        public void ticketsOnSale(Int32 p)
        {  // Event handler
            //create order object to send all values through a buffer for the airline to process
            //lock (multiCellBuffer)
            //{
            //    Order order = new Order();
            //    order.setAmount(25);
            //    multiCellBuffer.order = order;
            //    multiCellBuffer.setOneCell(order);
            //    Console.WriteLine("Travel Agency{0} tickets are on sale: as low as ${1} each", Thread.CurrentThread.Name, p);
            //}
            Console.WriteLine("Travel Agency{0} tickets are on sale: as low as ${1} each", Thread.CurrentThread.Name, p);
            onSaleFlag = 1;   
           //How am i supposed to generate an order for a thread if the thread does not exist here>
        }
    }

    //order processing class/ processorder method - check credit card method and calculate method/ register credit card array

    public class Order
    {
        private string senderId;
        private int cardNo;
        private int amount;
        private double unitPrice;

        public void setSender(string senderId) //thread name
        {
            this.senderId = senderId;
        }

        public void setCardNumber(int num) // numbers between 5000 - 7000
        {
            this.cardNo = num;
        }

        public void setAmount(int num)
        {
            this.amount = num;
        }

        public void setUnitPrice(double price)
        {
            this.unitPrice = price;
        }

        public string getSender()
        {
            return senderId;
        }

        public int getAmount()
        {
            return amount;
        }
    }

    public class MultiCellBuffer
    {
        public static Semaphore _pool;
        public Order order;

        public MultiCellBuffer()
        {
            _pool = new Semaphore(0, 2);
            
            Console.WriteLine("Semaphore Released{0} **", _pool.Release(2));
        }
        
        public void setOneCell(Order order)
        {
            lock (order)
            {
                _pool.WaitOne();
                this.order = order;

                Console.WriteLine("ORDER POOL IS NOW OCCUPIED BY TRAVEL AGENCY {0}", Thread.CurrentThread.Name); //Should increase semaphore by 1
                _pool.Release();



            }
            
        }

        public Order getOneCell()
        {
            //lock (order)
            //{
            
                Console.WriteLine("AIRLINE HAS RECEIVED THE ORDER FOR TRAVEL AGENCY -------------------------------------{0}", order.getSender());
                
            
           // }
            return order;


        }

        
    }





}

