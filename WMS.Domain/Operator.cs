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
                throw new BusinessException("Фамилия не задана");

            if (name == string.Empty)
                throw new BusinessException("Имя не задано");

            if (patronymic == string.Empty)
                throw new BusinessException("Отчество не задано");


            return new Operator(
                Guid.NewGuid(),
                surname,
                name,
                patronymic);
        }

        public void Update(string surname, string name, string patronymic)
        {
            if (string.IsNullOrWhiteSpace(surname))
                throw new BusinessException("Фамилия не задана");

            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessException("Имя не задано");

            if (string.IsNullOrWhiteSpace(patronymic))
                throw new BusinessException("Отчество не задано");


            Surname = surname.Trim();
            Name = name.Trim();
            Patronymic = patronymic.Trim();
        }


        public string FullName => $"{Surname} {Name?.FirstOrDefault()}. {Patronymic?.FirstOrDefault()}.";
    }
}
