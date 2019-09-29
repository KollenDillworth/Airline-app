using System;
using System.Threading;

namespace Airline_app
{
    public delegate void priceCutEvent(Int32 pr);
    class Program
    {
        public static Semaphore _pool;
        public static int onSaleFlag = 0;
        static void Main(string[] args)
        {
            _pool = new Semaphore(0, 2);
            MultiCellBuffer multiCellBuffer = new MultiCellBuffer();
            Airline airline = new Airline(multiCellBuffer);

            Thread airlineThread = new Thread(new ThreadStart(airline.airlineFunc));

            airlineThread.Start();         // Start one farmer thread

            TravelAgency travelAgency = new TravelAgency(multiCellBuffer);

            Airline.priceCut += new priceCutEvent(travelAgency.ticketsOnSale);

            Thread[] travelAgencies = new Thread[5];
            for (int i = 0; i < 5; i++)
            {   // Start N retailer threads
                travelAgencies[i] = new Thread(new ThreadStart(travelAgency.travelAgencyFunc));
                travelAgencies[i].Name = (i + 1).ToString();
                travelAgencies[i].Start();
            }
            Thread.Sleep(500);
            _pool.Release(2); //Opening up semaphore
            Console.WriteLine("Main thread exits.");


        }
    }

    public class Airline //Airline class
    {
        static Random rng = new Random();
        public static event priceCutEvent priceCut; // Define event
        private static Int32 chickenPrice = 50;
        private MultiCellBuffer multiCellBuffer;

        public Airline(MultiCellBuffer multiCellBuffer)
        {
            this.multiCellBuffer = multiCellBuffer;
        }

        public Int32 getPrice()
        {

            return chickenPrice;

        }

        public static void orderProcessing(Order order, int price)
        {
            //Checking order, if credit card information is correct then process order, if not then reject  
            if (order.getCardNumber() >= 5000 && order.getCardNumber() <= 7000)
            {
                double val = (double)order.getAmount() * (double)price;
                Console.WriteLine("--------------------------------------------------- ORDER HAS BEEN PROCESSED SUCCCESSFULLY FOR AGENCY {0}. Card No: {1}. Tickets Purchased: {2}. Total Price: ${3}. At Price ${4} ea", order.getSender(), order.getCardNumber(), order.getAmount(), val, order.getUnitPrice());
            }
            else
            {
                Console.WriteLine("--------------------------------------------------- CARD NUMBER FORMAT INCORRECT FOR AGENCY {0}. Card No: {1}. Price: ${2}", order.getSender(), order.getCardNumber(), order.getUnitPrice());
            }



        }

        public static void changePrice(Int32 price)
        {

            Program.onSaleFlag = 0; // static flag to handle the onsale event
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
                Airline.changePrice(p);

                if (multiCellBuffer.order != null)
                {

                    Console.WriteLine("--------------------------------------------------- Airline is Processing Order for Travel Agency {0}", multiCellBuffer.order.getSender());
                    
                    Thread link = new Thread(() => Airline.orderProcessing(multiCellBuffer.order, multiCellBuffer.price)); //Generate new order processing thread
                    link.Start();
                    multiCellBuffer.getOneCell();
                }

            }
        }
    }

    public class TravelAgency //Travel Agency Class
    {

        private MultiCellBuffer multiCellBuffer;
        static Random rng = new Random();
        public TravelAgency(MultiCellBuffer multiCellBuffer)
        {
            this.multiCellBuffer = multiCellBuffer;
        }

        public void createOrder(Order order, int price, string threadName)
        {
            //Creatint specific orders based off of thread name
            if (threadName.Equals("1"))
            {
                Console.WriteLine("--------------------------------------------------- Travel Agency {0} is Attempting to make an order for the price {1}", threadName, price);
                order.setAmount(rng.Next(1, 3));
                order.setCardNumber(rng.Next(4000, 7000));
                order.setSender(threadName);
                order.setUnitPrice((double)price);
                multiCellBuffer.setOneCell(order);
            }
            else if (threadName.Equals("2"))
            {
                Console.WriteLine("--------------------------------------------------- Travel Agency {0} is Attempting to make an order for the price {1}", threadName, price);
                order.setAmount(rng.Next(2, 4));
                order.setCardNumber(rng.Next(4000, 7000));
                order.setSender(threadName);
                order.setUnitPrice((double)price);
                multiCellBuffer.setOneCell(order);
            }
            else if (threadName.Equals("3"))
            {
                Console.WriteLine("--------------------------------------------------- Travel Agency {0} is Attempting to make an order for the price {1}", threadName, price);
                order.setAmount(rng.Next(1, 4));
                order.setCardNumber(rng.Next(4000, 7000));
                order.setSender(threadName);
                order.setUnitPrice((double)price);
                multiCellBuffer.setOneCell(order);
            }
            else if (threadName.Equals("4"))
            {
                Console.WriteLine("--------------------------------------------------- Travel Agency {0} is Attempting to make an order for the price {1}", threadName, price);
                order.setAmount(rng.Next(1, 5));
                order.setCardNumber(rng.Next(4000, 7000));
                order.setSender(threadName);
                order.setUnitPrice((double)price);
                multiCellBuffer.setOneCell(order);
            }
            else if (threadName.Equals("5"))
            {
                Console.WriteLine("--------------------------------------------------- Travel Agency {0} is Attempting to make an order for the price {1}", threadName, price);
                order.setAmount(rng.Next(1, 6));
                order.setCardNumber(rng.Next(4000, 7000));
                order.setSender(threadName);
                order.setUnitPrice((double)price);
                multiCellBuffer.setOneCell(order);
            }
        }

        public void travelAgencyFunc()
        {

            Order order = new Order();

            Airline airline = new Airline(multiCellBuffer);
            for (Int32 i = 0; i < 20; i++)
            {

                Thread.Sleep(500);

                Int32 p = airline.getPrice();
                Console.WriteLine("Travel Agency{0} has everyday low price: ${1} each", Thread.CurrentThread.Name, p);


                if (Program.onSaleFlag == 1)
                {

                    multiCellBuffer.price = p;
                    createOrder(order, multiCellBuffer.price, Thread.CurrentThread.Name);  //Generate order object if flash sale occurs

                }

            }

        }


        public void ticketsOnSale(Int32 p)
        {  // Event handler

            Console.WriteLine("Travel Agency{0} tickets are on sale: as low as ${1} each", Thread.CurrentThread.Name, p);

            Program.onSaleFlag = 1;  //Set flag to 1 if sale occurs to communicate with travel agency

        }
    }


    public class Order //Order class
    {
        private string senderId;
        private int cardNo;
        private int amount;
        private double unitPrice;

        //Basic getters and setters
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

        public int getCardNumber()
        {
            return cardNo;
        }

        public double getUnitPrice()
        {
            return unitPrice;
        }
    }

    public class MultiCellBuffer //Multicell buffer class
    {

        public Order order;
        int counter = 0;

        public int price { get; set; }
        public MultiCellBuffer()
        {

        }

        public void setOneCell(Order order)
        {

            lock (this)
            {
                //Call waitOne if counter is less than amout of cells avaliable
                if (counter < 2)
                {
                    
                    Program._pool.WaitOne();
                    this.order = order;
                    counter++;
                    Monitor.Wait(this);
                }
                else
                {
                    Monitor.Pulse(this);
                }
            }

        }

        public void getOneCell()
        {
            lock (this)
            {
                //Releasing pool
                if (order != null && counter > 0)
                {
                    Monitor.Pulse(this);
                    Program._pool.Release();
                    counter--;

                }
                else
                {
                    Monitor.Pulse(this);
                }
            }
        }

    }





}

