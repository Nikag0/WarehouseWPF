using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain
{
    public class Operator
    {
        public Guid Id { get; private set; }
        public string Surname { get; private set; }
        public string Name { get; private set; }
        public string Patronymic { get; private set; }

        private Operator() { } // для EF

        public Operator(string surname, string name, string patronymic)
        {
            Id = Guid.NewGuid();
            Surname = surname;
            Name = name;
            Patronymic = patronymic;
        }

        public string FullName => $"{Surname} {Name?.FirstOrDefault()}. {Patronymic?.FirstOrDefault()}.";
    }
}
