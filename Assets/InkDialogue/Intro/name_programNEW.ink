VAR show_name = ""

Carmen: Awww, muchas gracias jefecita!
#SFX_JINGLE INTERES
#EXPRESSION_LOLITA_SERIACENTRO1
Lolita: Quietas ahí.
Lucía: Y ahora qué?
Lolita: A dónde creen que van, no les parece que olvidan algo de suma importancia?
Carmen: Ehhh, cómo dices?
Lucía: De qué hablas Lolita?
#BG_STUDIO
#EXPRESSION_LOLITA_PREOCUPADA1
Lolita: Quizás estar en el set de grabación les refresque la memoria. 
#EXPRESSION_LOLITA_INQUISITIVA1
Lolita: Ya saben qué le falta a esta producción?
#EXPRESSION_LOLITA_CANSADA1
Lucía: La verdad no tengo la más mínima idea de qué podría ser.
Carmen: Yo tampoco.
#EXPRESSION_LOLITA_ENOJADA1
#WIGGLE_NORMAL
Lolita: EL NOMBRE DEL PROGRAMA!!
#NO_WIGGLE
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
    

* [No decidir nombre ahora]
    ~ show_name = ""
    Lucia: Mejor lo decidimos después...
    Carmen: ¿Estás segura? El público estará esperando...
    Lolita: La indecisión puede ser... peligrosa.
  // <- GATHER: aquí se "reúnen" las ramas y continúa la historia


{ show_name != "" :
    Lucía: Lo tengo! El programa se llamará "{show_name}"
    Lolita: Debo admitir, que esta vez te luciste.
    Carmen: "{show_name}" me gusta como suena eso! Sé que todos nos amarán!
- else:
    Lucia: Bien, aunque aún no tenemos nombre, podemos seguir adelante.
}

 
- 
#BG_ELEVADORES
Lolita: Espléndido, ahora sí podemos ir a comer. Conozco un gran lugar por el centro, yo invito muchachas!
Lucía: Espera, vienes con nosotras?
Lolita: Iré por mis llaves, no tardo. Pero al volver quiero verlas manos a la obra.
Carmen: Por supuesto, yo recibiré a los panelistas y guiaré a nuestros técnicos.
Lucía: Yo mientras iré a camerinos a alistarme.
Lolita: Y yo las estaré observando desde el monitor. Rómpanse una pierna ;)

#WIGGLE_NORMAL
Lucía: SEREMOS EL SHOW NÚMERO UNO!!!!
#NO_WIGGLE

Narrador: Y así, nuestras heroínas emprendieron su viaje en el glamuroso mundo de los reflectores y el estrellato.
Narrador: Qué les deparará el futuro? Solo hay una forma de averiguarlo!
 
-> END

= wait_for_custom_name
// Este knot espera que el DialogManager establezca show_name externamente NO BORRAR ESTE BLOQUE

{ show_name != "":
    Lucia: ¡Excelente elección! "{show_name}" tiene //potencial.
    Carmen: ¡Qué original! Definitivamente llamará la atención.
    Lolita: Un nombre único para un show único... *sonrisa misteriosa*
- else:
    Lucia: Hmm, parece que necesitas más tiempo para decidir...
}

// Después de los diálogos de respuesta, continúa con el resto
{ show_name != "":
    Lucía: Ehmm, el show se llamará "{show_name}"
    Carmen: ...
    Lolita: ...
    #WIGGLE_NORMAL
    Carmen: ME FASCINA!!! 
    #NO_WIGGLE
    Carmen: Definitivamente todos amarán "{show_name}", será un éxito rotundo!
    Lolita: Debo admitirlo, "{show_name}" es un título óptimo. Funciona.
}

-#BG_ELEVADORES
Lolita: Espléndido, ahora sí podemos ir a comer. Conozco un buen lugar por el centro, yo invito muchachas!
Lucía: Espera, vienes con nosotras?
Lolita: Iré por mis llaves, no tardo. Pero al volver quiero verlas manos a la obra.
#SFX_JINGLE PLATA
Carmen: Por supuesto, yo recibiré a los panelistas y guiaré a nuestros técnicos.
#SFX_JINGLE6
Lucía: Yo mientras iré a camerinos a alistarme.
#SFX_JINGLE4
Lolita: Y yo las estaré observando desde el monitor. Rómpanse una pierna ;)

#WIGGLE_NORMAL
Lucía: SEREMOS EL SHOW NÚMERO UNO!!!!
#NO_WIGGLE

Narrador: Y así, nuestras heroínas emprendieron su viaje en el glamuroso mundo de los reflectores y el estrellato.
Narrador: Qué les deparará el futuro? Solo hay una forma de averiguarlo!

-> DONE



