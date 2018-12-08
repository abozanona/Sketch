# Sketch

Sketch is a c# library for creating the database and GUI automatically for accounting descktop and mobile applications in c#.

Sketch creates custom Data Grid Viewes by connecting them with models using only attributes.

## How to use

 - Add the latest version of [sqlite-net-pcl](https://www.nuget.org/packages/sqlite-net-pcl/) from Nuget to your solution.
 - Specify your DB file location and models in `AppDatabase` file..
 - create a new instance of `AppDatabase` in your `Main` method of `Program` class.
 - When you create models, make sure that they extend `BaseModel`.
 - In forms, create a `DataGridView` and create new `DataGridViewEvents` class.

Code example:

    public enum UserType
    {
        [Description("Owner")]
        Owner,
        [Description("Employee")]
        Employee,
        [Description("Supplier")]
        Supplier,
        [Description("Customer")]
        Customer
    }
        [Table("User")]
    public class User: BaseModel
    {
        [Browsable(false)]
        [Column("id")]
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [DisplayName("Name")]
        [Column("name")]
        public string name { get; set; }

        [DisplayName("Company name")]
        [Column("companyName")]
        public string companyName { get; set; }

        [DisplayName("Mobile number")]
        [Column("mobileNumber")]
        public string mobileNumber { get; set; }

        public override string ToString()
        {
            return name;
        }

        public override BaseModel createEmpty()
        {
            return new User
            {
                companyName = "",
                location = "",
                mobileNumber = "",
                name = ""
            };
        }
        public override int _id
        {
            get { return id; }
            set { id = value; }
        }
    }
    [Table("Transaction")]
    public class Transaction: BaseModel
    {
        [Browsable(false)]
        [Column("id")]
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }

        [Column("userid")]
        [DisplayName("Select User")]
        [DBColumn(typeof(User))]
        [Browsable(true)]
        public int userId { get; set; }

        [DisplayName("Cost")]
        [Column("cost")]
        public double cost { get; set; }

        [DisplayName("Cost in dollers")]
        public double costInDollers { 
        	get { return cost * 3.7;}
        	set { cost = value / 3.7;}
    	}

        [DisplayName("User type")]
        [Column("usertype")]
        public UserType userType { get; set; }

        [DisplayName("Transaction date")]
        [Column("date")]
        public DateTime date { get; set; }

        public override BaseModel createEmpty()
        {
            return new Transaction
            {
            };
        }
        public override int _id
        {
            get { return id; }
            set { id = value; }
        }

        public override string ToString()
        {
            return "Transaction number " + id;
        }
    }

    DataGridViewEvents<Transaction> dgveTransaction
        = new DataGridViewEvents<Transaction>(dgv);
    btnAddTransaction.Click += dgveTransaction.btnAdd_Click;
