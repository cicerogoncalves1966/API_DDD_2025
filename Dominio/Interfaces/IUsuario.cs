﻿namespace Dominio.Interfaces
{
    public interface IUsuario
    {
        Task<bool> AdicionaUsuario(string email, string senha, int idade, string celular);
        Task<bool> ExisteUsuario(string email, string senha);
        Task<string> RetornaIdUsuario(string email);
    }
}
