using Dominio.Interfaces;
using Entidades.Entidades;
using Infraestrutura.Configuracoes;
using Infraestrutura.Repositorio.Genericos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infraestrutura.Repositorio
{
    public class RepositorioUsuario : RepositorioGenerico<ApplicationUser>, IUsuario
    {
        private readonly DbContextOptions<Contexto> _optionsBuilder;

        public RepositorioUsuario()
        {
            _optionsBuilder = new DbContextOptions<Contexto>();
        }

        public async Task<bool> AdicionaUsuario(string email, string senha, int idade, string celular)
        {
            try
            {
                using (var data = new Contexto(_optionsBuilder))
                {
                    await data.ApplicationUser.AddAsync(
                               new ApplicationUser
                               {
                                   Email = email,
                                   PasswordHash = senha,
                                   Idade = idade,
                                   Celular = celular
                               });
                    await data.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
