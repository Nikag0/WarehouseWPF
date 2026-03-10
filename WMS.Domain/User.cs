using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Surname { get; private set; }
        public string Name { get; private set; }
        public string Patronymic { get; private set; }

        private User() { } // для EF

        public User(string surname, string name, string patronymic)
        {
            Id = Guid.NewGuid();
            Surname = surname;
            Name = name;
            Patronymic = patronymic;
        }

        public string FullName => $"{Surname} {Name?.FirstOrDefault()}. {Patronymic?.FirstOrDefault()}.";
    }
}
