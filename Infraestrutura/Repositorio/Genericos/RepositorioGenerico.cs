using Dominio.Interfaces.Genericos;
using Infraestrutura.Configuracoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Infraestrutura.Repositorio.Genericos
{
    public class RepositorioGenerico<T> : IGenericos<T>, IDisposable where T : class
    {
        private readonly DbContextOptions<Contexto> _OptionsBuilder;

        public RepositorioGenerico()
        {
            _OptionsBuilder = new DbContextOptions<Contexto>();
        }

        public async Task Adicionar(T obj)
        {
            using (var data = new Contexto(_OptionsBuilder))
            { 
                await data.Set<T>().AddAsync(obj);
                await data.SaveChangesAsync();
            }
        }

        public async Task Atualizar(T obj)
        {
            using (var data = new Contexto(_OptionsBuilder))
            {
                data.Set<T>().Update(obj);
                await data.SaveChangesAsync();
            }
        }

        public async Task<T> BuscarPorId(int id)
        {
            using (var data = new Contexto(_OptionsBuilder))
            {
                return await data.Set<T>().FindAsync(id);
            }
        }

        public async Task Excluir(T obj)
        {
            using (var data = new Contexto(_OptionsBuilder))
            {
                data.Set<T>().Remove(obj);
                await data.SaveChangesAsync();
            }
        }

        public async Task<List<T>> Listar()
        {
            using (var data = new Contexto(_OptionsBuilder))
            {
                return await data.Set<T>().AsNoTracking().ToListAsync();
            }
        }

        #region Disposed https://docs.microsoft.com/pt-br/dotnet/standard/garbage-collection/implementing-dispose
        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //
            }

            disposed = true;
        }
        #endregion
    }
}
