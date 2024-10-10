# CFDataAccess

CFDataAccess is a simple and efficient data access library for interacting with SQL databases. It simplifies common database operations, making it easier to perform CRUD operations and more, using C#.

## Features

- Easy-to-use API for SQL database interaction
- Support for parameterized queries
- Connection management
- Built-in support for common data access patterns
- Compatible with .NET 8.0 and later

## Installation

You can install this library via NuGet Package Manager or using the .NET CLI:

### NuGet Package Manager:
In Visual Studio, go to **Tools** → **NuGet Package Manager** → **Manage NuGet Packages for Solution**, and search for `CFDataAccess`.

### .NET CLI:

```bash
dotnet add package CFDataAccess
````

### Package Manager Console:
```bash
Install-Package CFDataAccess
````
## Usage
Here's an example of how to use CFDataAccess to perform basic CRUD operations:
 #### 1. Basic Setup
  First, set up your connection string in your application’s configuration:
   ```bash
   {
      "ConnectionStrings": {
        "DefaultConnection": "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;"
      }
   }
````
  #### 2. Example Code
   ```bash
   using CFDataAccess;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

public class ExampleUsage
{
    private readonly DataAccess _dataAccess;

    public ExampleUsage(string connectionString)
    {
        _dataAccess = new DataAccess(connectionString);
    }

    // Retrieve data from the database
    public async Task<List<User>> GetUsersAsync()
    {
        var sql = "SELECT * FROM Users";
        return await _dataAccess.QueryAsync<User>(sql);
    }

    // Insert data into the database
    public async Task<int> AddUserAsync(User user)
    {
        var sql = "INSERT INTO Users (Name, Age) VALUES (@Name, @Age)";
        var parameters = new[]
        {
            new SqlParameter("@Name", user.Name),
            new SqlParameter("@Age", user.Age)
        };
        return await _dataAccess.ExecuteAsync(sql, parameters);
    }
}
````
 #### 3. Supported Methods
  - QueryAsync<T>(): Executes a query and maps the result to a list of the specified type.
  - ExecuteAsync(): Executes a command that modifies data (e.g., INSERT, UPDATE, DELETE).
  - Transaction support: Easily handle database transactions.
## License
This project is licensed under the MIT License.

## Contributing:

Feel free to submit issues and pull requests! Contributions are welcome.
Repository
The source code for this library is available at: https://github.com/goldi0002/CFDataAccess

### Instructions:
1. Copy the above content into a text file.
2. Save the file as `README.md` in the root of your project directory or repository.

This will ensure your project has a properly formatted `README.md` file that is easy 