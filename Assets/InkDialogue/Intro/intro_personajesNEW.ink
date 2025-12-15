VAR show_name = ""

// Texto inicial
#SHOW_LUCÍA_LEFT
Lucía: No puedo creerlo, hoy es el gran día. Y por fin soy la primera en llegar al estudio!
#SFX_JINGLE FUNNY
#EXPRESSION_LUCÍA_ASUSTADA1
#WIGGLE_NORMAL
???: LUCYYYY!!!
#NO_WIGGLE
#EXPRESSION_LUCÍA_CONFUNDIDA1
Lucía: (O quizás no...)
#SHOW_CARMEN_RIGHT
#EXPRESSION_LUCÍA_ENTRADA
Lucía: Hola Carmen, qué sorpresa verte aquí desde tan temprano.
#EXPRESSION_CARMEN_CONFIADA1
Carmen: Lo mismo digo Lucy, de hecho pasé toda la noche calibrando los equipos de producción.
#EXPRESSION_LUCÍA_IMPRESIONADA1
Lucía: Oh vaya, eso es dedicación!
#EXPRESSION_CARMEN_AVERGONZADA1
#EXPRESSION_LUCÍA_PREOCUPADA1
Carmen: Me quedé dormida en el cuarto de mantenimiento y me despertaron las alarmas de la bodega :D
Lucía: Ehhh... estás segura de que no quieres descansar un poco más?
#SFX_JINGLE POLVITOS
#EXPRESSION_LUCÍA_ASUSTADA1
#EXPRESSION_CARMEN_ACTIVADA1
Carmen: Descuida, esos polvitos raros que caían del techo me dejaron más activada que nunca!
#EXPRESSION_LUCÍA_PREOCUPADA2
Lucía: A veces me preocupas, mujer...
#SHOW_LOLITA_CENTER
#SFX_JINGLE LOLITA
Lolita: Buen día muchachas, todo listo para el gran estreno de hoy?
#EXPRESSION_CARMEN_FELIZ1
#EXPRESSION_LUCÍA_EMOCIONADA1
Lucía: Lo tenemos cubierto, Lolita. Gracias por estar al pendiente.
#EXPRESSION_LOLITA_SONRISACENTRO
Lolita: Me alegra escuchar eso.
#EXPRESSION_LUCÍA_ENTRADA
#EXPRESSION_CARMEN_HYPEADA1
Carmen: Será el debut más épico en la historia de la televisión!
#EXPRESSION_LOLITA_ALIVIADA1
Lolita: Ja, no cantemos victoria todavía. Eso lo dictará el público...
#EXPRESSION_LUCÍA_HYPEADA1
Lucía: Tienes razón, debemos dar nuestro máximo esfuerzo si queremos presentarle a nuestra audiencia un show inolvidable!
#EXPRESSION_CARMEN_FELIZ1
#SFX_JINGLE INTERES
#EXPRESSION_LOLITA_SONRISACENTRO
#EXPRESSION_LUCÍA_CONFUNDIDA1
Lolita: Aunque debo admitir que los ejecutivos de la cadena tienen altas expectativas respecto a tu proyecto, Lucía.
#EXPRESSION_LUCÍA_PREOCUPADA1
#EXPRESSION_CARMEN_HYPEADA1
Lucía: Oh vaya... 
#EXPRESSION_LOLITA_ENTRADA
Lolita: Pero hey, sin presiones.

-> contexto

=== contexto ===
* [Cuéntenme más del programa]
    #EXPRESSION_LUCÍA_CHISTE1
    #EXPRESSION_LOLITA_PREOCUPADA1
    Lolita: Seguramente debes estar bromeando, estamos a horas del estreno y no sabes de qué va tu show?
    #EXPRESSION_CARMEN_AVERGONZADA1
    Carmen: Solo es un chiste para aliviar la tensión, verdad Lucy? Jeje...
    #EXPRESSION_LUCÍA_EMOCIONADA1
    Lucía: Por favor, refrésquenme la memoria.
    #EXPRESSION_LOLITA_SERIACENTRO1
    Lolita: Está bien, te la dejo pasar porque me invitaste un encebollado la otra vez. Carmen, porfavor ponla al corriente.
    #EXPRESSION_CARMEN_CONFIADA1
    Carmen: Tranquila no es muy complicado, qué quieres saber exactamente?
    -> preguntas
    -> contexto
    
* [Quiénes son ustedes?]
    #EXPRESSION_CARMEN_TRISTE1
    #EXPRESSION_LOLITA_SERIACENTRO1
    Carmen: Oye, eso no es gracioso.
    #EXPRESSION_CARMEN_HYPEADA1
    Carmen: Sabes que soy tu Asistente de Producción y muy buena amiga tuya desde hace años!
    #EXPRESSION_LOLITA_SONRISACENTRO
    Lolita: Bueno, ya que insistes.
    #EXPRESSION_LOLITA_ENTRADA
    #EXPRESSION_CARMEN_AVERGONZADA1
    Lolita: Mi nombre es Lola Cortez, tengo 33 años de edad. Trabajo de Directora de Producción en el departamento de Horario Estelar del prestigioso Canal 13.
    #EXPRESSION_LOLITA_ALIVIADA1
    Lolita: Actualmente me encuentro soltera. Mi casa se encuentra en la sección noroeste de Mo-
    #EXPRESSION_LUCÍA_ASUSTADA1
    Lucía: Ehhh, ok, ok ya entendí.
    -> contexto
    
* [Lolita, por qué eres verde?]
    #EXPRESSION_LUCÍA_CHISTE1
    #EXPRESSION_CARMEN_RISA1
    #EXPRESSION_LOLITA_CANSADA1
    Lolita: Ja ja ja, ay pero qué ocurrente. 
    #EXPRESSION_LOLITA_ENOJADA1
    #EXPRESSION_LUCÍA_ASUSTADA1
    #EXPRESSION_CARMEN_TRISTE1
    #SFX_FAIL SOFT
    Lolita: Y si mejor vas y le cuentas ese chiste a Recursos Humanos? Estoy segura de que les fascinará.
    #EXPRESSION_LUCÍA_EMOCIONADA1
    #EXPRESSION_CARMEN_CHISMOSA1
    Carmen: *susurrando* Yo tengo la teoría de que es porque toma mucho matcha!
    #EXPRESSION_LOLITA_PREOCUPADA1
    #EXPRESSION_LUCÍA_CONFUNDIDA1
    Lolita: Puedo escucharlas... ash como sea.
    -> contexto
    

 *-> dilema   
    
=== preguntas ===

* [De qué trata el show?]
    #EXPRESSION_CARMEN_CONFIADA1
    Carmen: La premisa gira entorno a que las personas acudan a nuestro programa. 
    #EXPRESSION_CARMEN_FELIZ1
    Carmen: Para que los ayudemos a resolver problemas que afectan sus vidas cotidianas.
    #EXPRESSION_LOLITA_ENTRADA
    Lolita: Una propuesta muy revolucionaría, si me lo preguntan.
    #EXPRESSION_LUCÍA_CHISTE1
    Lucía: Suena muy cliché, a quién se le ocurrió esa tontería?
    #EXPRESSION_CARMEN_TRISTE1
    Carmen: ...
    #EXPRESSION_LOLITA_PREOCUPADA1
    Lolita: ...
    Carmen: A ti, Lucía.
    #EXPRESSION_LUCÍA_PREOCUPADA2
    Lucía: Oh...
    -> preguntas

* [Cuál es mi rol dentro del show?]
    #EXPRESSION_CARMEN_HYPEADA1
    Carmen: Eres la Presentadora! La estrella del programa!
    Carmen: Tú tratas con los invitados y mantienes contento al público.
    Lolita: Tus decisiones influirán en el desarrollo del conflicto y las reacciones de la audiencia. 
    Lolita: Como puedes observar, es una gran responsabilidad.
    Carmen: Puedes revisar tus Niveles de Rating en vivo y en directo!!!
    Lolita: Sé que te irá de maravilla, Lucía.
    -> preguntas

* [Cuánto me van a pagar?] 

    Lolita: Todo depende del rating que alcances en cada capítulo.
    Lolita: Si llegas a...
    #EXPRESSION_CARMEN_HYPEADA1
    #EXPRESSION_LOLITA_SONRISACENTRO
    #EXPRESSION_LUCÍA_EMOCIONADA1
    #WIGGLE_NORMAL
    Lolita: 100 PUNTOS DE RATING!!!! 
    #NO_WIGGLE
    Lolita: Recibirás un jugoso cheque y el show será renovado para una nueva temporada.
    #EXPRESSION_LOLITA_PREOCUPADA1
    Lolita: Por el contrario... 
    #EXPRESSION_LOLITA_SERIACENTRO1
    #EXPRESSION_CARMEN_TRISTE1
    #EXPRESSION_LUCÍA_ASUSTADA1
    #WIGGLE_NORMAL
    Lolita: SI LLEGAS A 0 PUNTOS DE RATING!!!!
    #NO_WIGGLE
    Lolita: Lamentablemente la cadena se verá en la obligación de CANCELAR el programa.
    Lolita: Y no recibirás ni un centavo.
    Carmen: Ay nooo :c 
    Lolita: Lo siento chicas, yo no hago las reglas. Solo sigo órdenes de los ejecutivos del Canal 13.
    #EXPRESSION_LUCÍA_PREOCUPADA1
    Lucía: Tranquila Lolita, entiendo.
    -> preguntas
    
*-> contexto


=== dilema ===
#EXPRESSION_CARMEN_FELIZ1
#EXPRESSION_LUCÍA_DEFAULT
- Carmen: Bueno si esas fueron tus dudas, creo que podemos continuar con los preparativos.
#EXPRESSION_LOLITA_SONRISACENTRO
Lolita: El tiempo es oro, al fin y al cabo, el mundo del espectáculo es todo un negocio.
#EXPRESSION_CARMEN_TRISTE1
Carmen: Espera un momento, no lo entiendes Lolita.
#EXPRESSION_CARMEN_ACTIVADA1
Carmen: Cuando estamos al aire, tenemos el poder de dar un mensaje.
#EXPRESSION_LOLITA_CANSADA1
Lolita: Ay, ya va a comenzar otra vez...
#EXPRESSION_LUCÍA_CONFUNDIDA1
Lucía: Creo que Carmen tiene un punto.
#EXPRESSION_CARMEN_RISA1
Carmen: Tú sabes a lo que me refiero!
#EXPRESSION_LOLITA_SERIACENTRO1
#EXPRESSION_LUCÍA_ASUSTADA1
Lolita: A ver, dinos qué piensas hacer con el "superpoder" de la televisión?

*[Quiero ayudar a las personas!]
#EXPRESSION_LUCÍA_HYPEADA1
#SFX_JINGLE CORRECTO
#EXPRESSION_CARMEN_FELIZ1
#EXPRESSION_LOLITA_ALIVIADA1
    Carmen: Así se habla!

*[Quiero hacer plata!]
#EXPRESSION_CARMEN_TRISTE1
#EXPRESSION_LUCÍA_HYPEADA1
#SFX_JINGLE CORRECTO
#EXPRESSION_LOLITA_SONRISACENTRO
    Lolita: Esa es la actitud!
    
 
-#EXPRESSION_LUCÍA_EMOCIONADA1
#EXPRESSION_CARMEN_AVERGONZADA1
#EXPRESSION_LOLITA_SERIACENTRO1
Lucía: Dejemos la charla para después, Carmencita salgamos a comer algo y a que te dé el sol. Te veo pálida.

 -> END
