using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DeliverooApp.Models;

public class Associazione
{
    public int NumOrdini { get; set; }
    public int IdArticoloX { get; set; }
    public int IdArticoloY { get; set; }

    public override string ToString()
    {
        return IdArticoloX + " " + IdArticoloY;
    }
}