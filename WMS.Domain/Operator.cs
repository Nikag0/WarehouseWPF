using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain.ExceptionControl;
using WMS.Domain.Interfaces;

namespace WMS.Domain
{
    public class Operator : ISoftDeletable
    {
        public Guid Id { get; private set; }
        public string Surname { get; private set; }
        public string Name { get; private set; }
        public string Patronymic { get; private set; }
        public bool IsDeleted { get; private set; }

        public void Delete()
        {
            IsDeleted = true;
        }

        private Operator() { } // для EF

        private Operator(Guid id, string surname, string name, string patronymic)
        {
            Id = id;
            Surname = surname;
            Name = name;
            Patronymic = patronymic;
        }

        public static Operator Create(
            string surname,
            string name,
            string patronymic)
        {
            if (surname == string.Empty)
                throw new OverallDomainException("Surname не задан");

            if (name == string.Empty)
                throw new OverallDomainException("Name не задан");

            if (patronymic == string.Empty)
                throw new OverallDomainException("Patronymic не задан");


            return new Operator(
                Guid.NewGuid(),
                surname,
                name,
                patronymic);
        }

        public void Update(string surname, string name, string patronymic)
        {
            if (string.IsNullOrWhiteSpace(surname))
                throw new OverallDomainException("Surname не задан");

            if (string.IsNullOrWhiteSpace(name))
                throw new OverallDomainException("Name не задан");

            if (string.IsNullOrWhiteSpace(patronymic))
                throw new OverallDomainException("Patronymic не задан");

            Surname = surname.Trim();
            Name = name.Trim();
            Patronymic = patronymic.Trim();
        }


        public string FullName => $"{Surname} {Name?.FirstOrDefault()}. {Patronymic?.FirstOrDefault()}.";
    }
}
