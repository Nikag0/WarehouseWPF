using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
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
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public bool IsDeleted { get; private set; } 

        private Operator() { } // для EF

        private Operator(Guid id, string surname, string name, string patronymic, DateTime createdAt, DateTime updatedAt)
        {
            Id = id;
            Surname = surname;
            Name = name;
            Patronymic = patronymic;

            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        public static Operator Create(
            string surname,
            string name,
            string patronymic)
        {
            Validate(surname, name, patronymic);

            var now = DateTime.UtcNow;

            return new Operator(
                Guid.NewGuid(),
                surname,
                name,
                patronymic,
                now,
                now);
        }

        public void Update(string surname, string name, string patronymic)
        {
            Validate(surname, name, patronymic);

            UpdatedAt = DateTime.UtcNow;

            Surname = surname.Trim();
            Name = name.Trim();
            Patronymic = patronymic.Trim();
        }

        private static void Validate(
            string surname,
            string name,
            string patronymic)
        {
            if (string.IsNullOrWhiteSpace(surname))
                throw new BusinessException("Фамилия не задана");

            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessException("Имя не задано");

            if (string.IsNullOrWhiteSpace(patronymic))
                throw new BusinessException("Отчество не задано");
        }

        public void Delete()
        {
            if (IsDeleted)
                return;

            IsDeleted = true;
        }

        public string FullName =>$"{Surname} {Name[0]}. {Patronymic[0]}.";
    }
}
