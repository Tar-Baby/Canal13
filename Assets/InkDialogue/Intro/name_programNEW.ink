VAR show_name = ""

Carmen: Awww, muchas gracias jefecita!
Lolita: Quietas ahí.
Lucía: Y ahora qué?
Lolita: A dónde creen que van, no les parece que olvidan algo de suma importancia?
Carmen: Ehhh, cómo dices?
Lucía: De qué hablas Lolita?
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
    Carmen: 7 palabras... E S E N C I A
    Lolita: No está nada mal.
    Carmen: Definitivamente todos amarán "{show_name}", será un éxito rotundo!


* [El Gran Chongo]
    ~ show_name = "El Gran Chongo"
    Lucía: El show se llamará "{show_name}"
    Carmen: ES FANTÁSTICO, todos querrán vernos en "{show_name}"!!!
    Lolita: Hmmm...dará de qué hablar sin duda.


* [Escribir nombre]
    Carmen: ¡Perfecto! Escribe el nombre que quieras para nuestro show.
    -> wait_for_custom_name
    

//* [No decidir nombre ahora]
//    ~ show_name = ""
//    Lucia: Mejor lo decidimos después...
//    Carmen: ¿Estás segura? El público estará//esperando...
   // Lolita: La indecisión puede ser... peligrosa.

-  Lolita: Espléndido, ahora sí podemos ir a comer. Conozco un gran lugar por el centro, yo invito muchachas!
Lucía: Espera, vienes con nosotras?
Lolita: Iré por mis llaves, no tardo.
Carmen: Genial, al regresar yo recibiré a los panelistas y guiaré a nuestros técnicos.
Lucía: Perfecto, yo mientras iré a camerinos a alistarme.
Lolita: Y yo Las estaré observando desde el monitor. Rómpanse una pierna ;)

#WIGGLE_NORMAL
Lucía: SEREMOS EL SHOW NÚMERO UNO!!!!
#NO_WIGGLE

Narrador: Y así, nuestras heroínas emprendieron su viaje en el glamuroso mundo de los reflectores y el estrellato.
Narrador: Qué les deparará el futuro? Solo hay una forma de averiguarlo!
 // <- GATHER: aquí se "reúnen" las ramas y continúa la historia

//{ show_name != "" :
//    Lucia: Bien, el programa "{show_name}" está //listo para comenzar.
//- else:
 //   Lucia: Bien, aunque aún no tenemos nombre, //podemos seguir adelante.
//}

* -> END

= wait_for_custom_name
// Este knot espera que el DialogManager establezca show_name externamente

// Después de los diálogos de respuesta, continúa con el resto
{ show_name != "":
        Lucía: Ehmm...el show se llamará "{show_name}"
        Carmen: ...
        Lolita: ...ese nombre...
        Carmen: ES FANTÁSTICO, ME ENCANTA, ME ENCANTAAAA!!!!
        Lolita: Hmmm...debo admitirlo, no está nada mal.
        Carmen: Definitivamente todos amarán "{show_name}", será un éxito rotundo!
}

-> DONE



