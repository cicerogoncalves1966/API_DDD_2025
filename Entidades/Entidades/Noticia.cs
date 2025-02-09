using Entidades.Notificacoes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entidades.Entidades
{
    [Table("TB_NOTICIA")]
    public class Noticia : Notifica
    {
        [Column("NTC_ID")]
        public int Id { get; set; }

        [Column("NTC _TITULO")]
        [MaxLength(255)]
        public string Titulo { get; set; }

        [MaxLength(1024)]
        [Column("NTC _INFORMACAO")]
        public string Informacao { get; set; }

        [Column("NTC _ATIVO")]
        public bool Ativo { get; set; }

        [Column("NTC _DATA_CADASTRO")]
        public DateTime? DataCadastro { get; set; }

        [Column("NTC _DATA_ALTERACAO")]
        public DateTime? DataAlteracao { get; set; }

        [ForeignKey("ApplicationUser")]
        [Column(Order = 1)]
        public string UserId { get; set; }

        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
