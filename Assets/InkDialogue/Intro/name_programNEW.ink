VAR show_name = ""

// Texto inicial
Lucía: Entendido, tal parece que ya tenemos todo en orden. Vamos por un encebollado, yo invito <3
Carmen: Ayyy, muchas gracias jefecita!
Lolita: Quietas ahí.
Lucía: (Chale, nos pilló saliendo antes de la hora de almuerzo.)
Lolita: A dónde creen que van, no les parece que olvidan algo de suma importancia?
Lolita: Quizás estar en el set de grabación les refresque la memoria. Ya saben qué le falta a esta producción?
Lucía: La verdad no tengo la más mínima idea de qué podría ser.
Carmen: Yo tampoco.
Lolita: EL NOMBRE DEL PROGRAMA!!
Carmen: Ayyy, ciertooooo.
Lucía: Hmmm, qué nombre puedo ponerle al show?


+ [Caso Piteado]
    ~ show_name = "Caso Piteado"
    Lucía: El show se llamará "{show_name}"
    Carmen: ...
    Lolita: ...ese nombre...
    Carmen y Lolita: 7 palabras... E S E N C I A
    Carmen: Definitivamente todos amarán "{show_name}", será un éxito rotundo!


* [El Gran Chongo]
    ~ show_name = "El Gran Chongo"
    Lucía: El show se llamará "{show_name}"
    Carmen: ES FANTÁSTICO, todos querrán vernos en "{show_name}"!!!
    Lolita: Hmmm...dará de qué hablar sin duda.


* [Escribir nombre]
    Lolita: ¡Perfecto! Escribe el nombre que quieras para nuestro show.
    -> wait_for_custom_name
    

* [No decidir nombre ahora]
    ~ show_name = ""
    Lucia: Mejor lo decidimos después...
    Carmen: ¿Estás segura? El público estará esperando...
    Lolita: La indecisión puede ser... peligrosa.

-   // <- GATHER: aquí se "reúnen" las ramas y continúa la historia

{ show_name != "" :
    Lucia: Bien, el programa "{show_name}" está listo para comenzar.
- else:
    Lucia: Bien, aunque aún no tenemos nombre, podemos seguir adelante.
}

Lolita: Está bien chicas, ahora sí vamos a comer. Conozco un buen lugar por el centro.
Carmen: ...Espera, vienes con nosotras?
Lolita: Iré por mis llaves, no tardo.


* -> END

= wait_for_custom_name
// Este knot espera que el DialogManager establezca show_name externamente
{ show_name != "":
    Lucia: ¡Excelente elección! "{show_name}" tiene potencial.
    Carmen: ¡Qué original! Definitivamente llamará la atención.
    Lolita: Un nombre único para un show único... *sonrisa misteriosa*
- else:
    Lucia: Hmm, parece que necesitas más tiempo para decidir...
}

// Después de los diálogos de respuesta, continúa con el resto
{ show_name != "":
        Lucía: Ehmm...el show se llamará "{show_name}"
        Carmen: ...
        Lolita: ...ese nombre...
        Carmen: ES FANTÁSTICO, ME ENCANTA, ME ENCANTAAAA!!!!
        Lolita: Hmmm...debo admitirlo, no está nada mal.
        Carmen: Definitivamente todos amarán "{show_name}", será un éxito rotundo!
}


Lolita: Está bien chicas, ahora sí vamos a comer. Conozco un buen lugar por el centro.
Carmen: ...Espera, vienes con nosotras?
Lolita: Iré por mis llaves, no tardo.

-> END

