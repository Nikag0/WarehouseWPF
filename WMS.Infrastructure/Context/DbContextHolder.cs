using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Infrastructure.Context
{
    public class DbContextHolder : IDbContextAccessor
    {
        private static readonly AsyncLocal<AppDbContext?> _currentContext = new();

        public AppDbContext CurrentContext
        {
            get => _currentContext.Value ?? throw new InvalidOperationException(
                "Контекст не инициализирован для текущего потока.");
            set => _currentContext.Value = value;
        }

        // Если значение внутри AsyncLocal не null, значит uow.Create() уже был вызван выше по стеку
        public bool IsContextInitialized => _currentContext.Value != null;
    }
}