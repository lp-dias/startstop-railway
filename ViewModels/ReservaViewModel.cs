using System;
using System.ComponentModel.DataAnnotations;

namespace StartStop.ViewModels
{
    public class ReservaViewModel
    {
        [Required(ErrorMessage = "O veículo é obrigatório.")]
        public int VeiculoId { get; set; }

        [Required(ErrorMessage = "Informe a data de início.")]
        [DataType(DataType.DateTime)]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "Informe a data de término.")]
        [DataType(DataType.DateTime)]
        public DateTime DataFim { get; set; }
    }
}
