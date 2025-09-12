VAR show_name = ""

// Texto inicial
Lucia: Entendido, tal parece que ya tenemos todo en orden. Vamos por un encebollado, yo invito <3
Carmen: ¡Muchas gracias jefecita!
Lucia: (Chale, nos pilló saliendo antes de la hora de almuerzo.)
Lolita: ¿A dónde creen que van, no les parece que olvidan algo de suma importancia?
Lucia: La verdad no tengo la más mínima idea de qué podría ser.
Carmen: Yo tampoco.
Lolita: Quizás estar en el set de grabación les refresque la memoria. ¿Ya saben qué le falta a esta producción?
Lolita: ¡¡EL NOMBRE DEL PROGRAMA!!
Carmen: Ayyy, ciertooooo.
Lucia: ¿Cómo pude pasar por alto algo tan fundamental?


* [Caso Piteado]
    ~ show_name = "Caso Piteado"
    Lucía: El show se llamará "{show_name}"
    Carmen: ...
    Lolita: ...ese nombre...
    Carmen y Lolita: 7 palabras... E S E N C I A


* [El Gran Chongo]
    ~ show_name = "El Gran Chongo"
    Lucia: ¡¡¡¡ES FANTÁSTICO, ME ENCANTA!!!
    Carmen:  ¡"{show_name}"! ¡Es perfecto para nuestro primer show!
    Lolita: Un debut memorable requiere preparación... *sonidos de aprobación*

* [Escribir nombre]
    Lucia: ¡Perfecto! Escribe el nombre que quieras para nuestro show.
    -> wait_for_custom_name
    

* No decidir nombre ahora
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
Carmen: ...¿Espera, vienes con nosotras?
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
{ show_name != "" :
     Lucia: ¡Excelente elección! "{show_name}" tiene potencial.
        Carmen: ¡Qué original! Definitivamente llamará la atención.
        Lolita: Un nombre único para un show único... *sonrisa misteriosa*
- else:
    Lucia: Hmm, parece que necesitas más tiempo para decidir...
}

Lolita: Está bien chicas, ahora sí vamos a comer. Conozco un buen lugar por el centro.
Carmen: ...¿Espera, vienes con nosotras?
Lolita: Iré por mis llaves, no tardo.

-> END

