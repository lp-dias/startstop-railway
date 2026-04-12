using System;
using System.ComponentModel.DataAnnotations;

namespace StartStop.ViewModels
{
    public class ReservaViewModel
    {
        [Required]
        public int VeiculoId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DataInicio { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DataFim { get; set; }
    }
}
