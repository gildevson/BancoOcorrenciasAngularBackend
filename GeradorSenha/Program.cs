using System;

class Program {
    static void Main() {
        var hash = BCrypt.Net.BCrypt.HashPassword("123456");
        Console.WriteLine(hash);
    }
}
