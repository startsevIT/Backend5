using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder();
//���������� CORS
builder.Services.AddCors();

var app = builder.Build();
//������������� CORS 
app.UseCors(param => param.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

//��������� ������
app.MapGet("/orders", () => 
{ 
    using Repository repo = new Repository();
    return repo.ReadAll();
});
//��������� ���������� ��������
app.MapGet("/orders/{id}", (Guid id) =>
{
    using Repository repo = new Repository();
    return repo.Read(id);
});
//�������� ��������
app.MapPost("/orders",(CreateOrderDTO dto) => 
{ 
    using Repository repo = new Repository();
    Order order = new Order(dto.Device, dto.ProblemType, dto.Description, dto.Client);
    repo.Add(order);
    repo.SaveChanges();
});
//�������������� ��������
app.MapPut("/orders/{id}", (UpdateOrderDTO dto,Guid id) =>
{
    using Repository repo = new Repository();
    repo.Update(dto, id);
    repo.SaveChanges();
});

//�� 5
//��������� ����������
app.MapGet("/statistics", () =>
{
    using Repository repo = new Repository();
    var competeCount = repo.GetCompleteCount();
    var averageTime = repo.GetAverageTime();
    var stat = repo.GetStatistics();
    StatisticDTO statistic = new StatisticDTO(competeCount,averageTime,stat);
    return statistic;
});
app.Run();

//�������� ������ ������� �������� ��
class Order
{
    public Order(string device, string problemType, string description, string client)
    {
        Id = Guid.NewGuid();
        StartDate = DateTime.Now;
        EndDate = null;
        Device = device;
        ProblemType = problemType;
        Description = description;
        Client = client;
        Status = "� ��������";
        Worker = "�� ��������";
        Comment = "";
    }

    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    //�� 5
    public DateTime? EndDate { get; set; }
    public string Device {  get; set; }
    public string ProblemType { get; set; }
    public string Description { get; set; }
    public string Client {  get; set; }
    public string status;
    //�� 5
    public string Status { 
        get => status; 
        set 
        { 
            if(value=="���������") 
                EndDate = DateTime.Now;
            status = value;
        } 
    }
    public string Worker { get; set; }
    public string Comment { get; set; }

}
record class CreateOrderDTO(
    string Device, 
    string ProblemType, 
    string Description, 
    string Client);
record class UpdateOrderDTO(
    string Status, 
    string Description,
    string Worker,
    string Comment);
//�� 5
record class StatisticDTO(
    int CompleteCount, 
    double AverageTime, 
    Dictionary<string, int> Stat);

class Repository : DbContext
{ 
    private DbSet<Order> Orders {  get; set; }
    public Repository()
    { 
        Orders = Set<Order>();
        Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=orders.db");
    }


    public void Add(Order order)
    { 
        Orders.Add(order);
    }
    public Order Read(Guid id)
    { 
        return Orders.Find(id);
    }
    public List<Order> ReadAll()
    { 
        return Orders.ToList();
    }
    //������� ������ ����
    public void Update(UpdateOrderDTO dto, Guid id)
    {
        Order order = Read(id);
        if(dto.Status != order.Status)
            order.Status = dto.Status;
        if(dto.Description != "")
            order.Description = dto.Description;
        if(dto.Worker != "")
            order.Worker = dto.Worker;
        if(dto.Comment != "")
            order.Comment = dto.Comment;
    }




    public int GetCompleteCount()
    {
        return Orders.Count(x => x.Status == "���������");
    }
    public double GetAverageTime()
    {
        List<Order> completeOrders = Orders.ToList().FindAll(x => x.Status == "���������");
        if (completeOrders.Count == 0)
            return 0;
        double allTime = 0;
        foreach (Order order in completeOrders)
            allTime += (order.EndDate - order.StartDate).Value.Hours;
        return allTime/completeOrders.Count;
    }
    public Dictionary<string, int> GetStatistics()
    { 
        Dictionary<string,int> result = new Dictionary<string, int>();
        foreach (Order order in Orders.ToList())
        { 
            if(result.ContainsKey(order.ProblemType))
                result[order.ProblemType]++;
            else
                result[order.ProblemType] = 1;
        }
        return result;
    }
}