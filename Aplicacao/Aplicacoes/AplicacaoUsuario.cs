﻿using Aplicacao.Interfaces;
using Dominio.Interfaces;

namespace Aplicacao.Aplicacoes
{
    public class AplicacaoUsuario : IAplicacaoUsuario
    {
        IUsuario _IUsuario;

        public AplicacaoUsuario(IUsuario iUsuario)
        {
            _IUsuario = iUsuario;
        }

        public async Task<bool> AdicionaUsuario(string email, string senha, int idade, string celular)
        {
            return await _IUsuario.AdicionaUsuario(email, senha, idade, celular);
        }

        public async Task<bool> ExisteUsuario(string email, string senha)
        {
            return await _IUsuario.ExisteUsuario(email, senha);
        }

        public async Task<string> RetornaIdUsuario(string email)
        {
            return await _IUsuario.RetornaIdUsuario(email);
        }
    }
}
