VAR show_name = ""
VAR episode_rating = 0  //luego agregar public_reaction
VAR public_reaction = "neutral"

//#FADEALL
// Inicio del programa principal
#MUSIC_DEFAULT SHOW THEME
Narrador: Al aire en 3...2...1...
#SFX_AUDIENCE CHEERING 2_REVERB
#SHOW_LUCÍA_LEFT
//#EXPRESSION_LUCÍA_SALUDO este es su expresion default en este caso
Lucía: Hola a todos, bienvenidos al gran estreno del nuevo show estelar de CANAL 13. El día de hoy tenemos un programa espectacular! 
#EXPRESSION_LUCÍA_FRASEALT
Lucía: Y quisiera comenzar con una frase, se trata de un antiguo proverbio montubio que dice así:
#EXPRESSION_LUCÍA_FRASE
Lucía: "El que la hace, se olvida. El que la recibe, nunca"
#EXPRESSION_LUCÍA_FELIZOJOSCERRADOS
Lucía: Que pase nuestra invitada especial!
#EXPRESSION_LUCÍA_QUE PASEEE
#SFX_AUDIENCE CHEERING 1_REVERB
#WIGGLE_NORMAL
Lucía: UN APLAUSO PARA ROCÍOOOOO!!!
#NO_WIGGLE

Narrador: El público enloquece.
#SHOW_ROCÍO_RIGHT
Rocío: Hola Lucía es un honor estar ante ustedes y ante las cámaras. Gracias por recibirme en tu programa.

#EXPRESSION_LUCÍA_NORMAL
Lucía: El placer es todo mío, bueno cuéntanos qué te trae aquí hoy.

#EXPRESSION_ROCÍO_NORMAL
Rocío: Verás Lucía, estoy enfrentando la decisión más difícil en la vida de toda mujer.

// OPCIONES DEL USUARIO (en rosado en el diagrama)
* [Vaya, si lo pones así... dime más!!!]
    #SFX_RATING UP SMALL_ECHO
    ~ episode_rating += 5
    // Esta es la respuesta del usuario/jugador
    -> continue_story //usar estos Diverts para sacar el Final Bueno y Final Malo

* [Ay, tampoco exageres pues mijita.]
    ~ episode_rating += 10
    #SFX_RATING UP MEDIUM_ECHO
    // Esta es la respuesta del usuario/jugador
    -> continue_story

= continue_story
// Aquí continúa independientemente de la opción elegida
#EXPRESSION_ROCÍO_ENAMORADA1
#EXPRESSION_LUCÍA_CONMOVIDASOFT
#SFX_AUDIENCE AWWW 1_REVERB
Rocío: Estoy enamorada...
// reaccion del publico Ternura
#EXPRESSION_ROCÍO_ENAMORADA2
#EXPRESSION_LUCÍA_SORPRENDIDA1
#SFX_AUDIENCE SHOCK 1_REVERB
Rocío: De dos a la vez... 
// reaccion del publico Asombro

#EXPRESSION_LUCÍA_CONFUNDIDA1
Lucía: Espérate, pérate. Cómo es eso?
#EXPRESSION_ROCÍO_BANDIDA1
Rocío: Tal y como escuchaste Lucía. Llevo saliendo ya un buen tiempo con dos chicos que conocí en la academia de baile en la que estudio.
#EXPRESSION_ROCÍO_FELIZINDECISA
#EXPRESSION_LUCÍA_DUDOSA1
Rocío:  Y la verdad es que ambos me hacen muy feliz. Los amo a los dos!
//#HIDE_LUCÍA para probar que funcione el FadeOut
* [Tranquila reina nosotros te ayudaremos a resolver este triángulo amoroso.]
    ~ episode_rating += 5
    #SFX_RATING UP SMALL_ECHO
    #EXPRESSION_ROCÍO_ENAMORADA3
    #EXPRESSION_LUCÍA_CONMOVIDASOFT
    Rocío: Gracias Lucía, sabía que podía contar con tu apoyo!
    
* [Los amas a los dos, Rocío? Hmm... no lo sé, algo me huele raro aquí.]
    ~ episode_rating += 10
    #SFX_RATING UP MEDIUM_ECHO
    #EXPRESSION_ROCÍO_NERVIOSA1
    #EXPRESSION_LUCÍA_CONFUNDIDA1
    Rocío: Ay no seas así, déjame explicarte antes de que saltes a conclusiones.
    
    
- #EXPRESSION_LUCÍA_FELIZOJOSCERRADOS
Lucía: Bueno, bueno cómo es la vaina?
#EXPRESSION_ROCÍO_ENTRADA
Rocío: Mira, la razón por la que tengo dos novios es sencilla. Tengo a uno para el Gusto y otro para el Gasto. 
    //~ episode_rating += 10 // reaccion del publico Asombro public_reaction = "asombro"

* [Tal y cómo lo sospeché. Eres una bandida!]
    ~ episode_rating += 10   // reaccion del publico Risas
    #SFX_RATING UP MEDIUM_ECHO
    #SFX_AUDIENCE SMALL LAUGHTER 1_REVERB
    #EXPRESSION_LUCÍA_PAPEADORA1
    #EXPRESSION_ROCÍO_TRISTE1
    Rocío: Lucíaaaa no me digas así!!
    
* [No sé si termino de comprender, pero tengo miedo de preguntar.]
    ~ episode_rating -= 10
    #SFX_RATING DOWN MEDIUM_ECHO
    #EXPRESSION_LUCÍA_DUDOSA2
    #EXPRESSION_ROCÍO_DUDOSA1
    Rocío: Ash, no es tan complicado.
    
-#EXPRESSION_ROCÍO_ENOJADA1
#EXPRESSION_LUCÍA_INCOMODA1
Rocío: ...Además, no se supone que en tu show ayudas a las personas?
#EXPRESSION_ROCÍO_ENOJADA2
#EXPRESSION_LUCÍA_INCOMODA2
Rocío: Qué culpa tengo yo de que el guapo sea chiro y el del billete sea bagre?!
#EXPRESSION_LUCÍA_RESIGNADA1
#EXPRESSION_ROCÍO_AVERGONZADA1
Rocío: Por eso quiero que me ayudes a decidirme por uno... 
Lucía: Ya ya, posi posi... 
#EXPRESSION_LUCÍA_MOTIVADA1
#EXPRESSION_ROCÍO_AVERGONZADA2
Lucía: La plena que esto se pone cada vez mejor.
#EXPRESSION_LUCÍA_QUE PASEEE
#EXPRESSION_ROCÍO_ENAMORADA4
#SFX_AUDIENCE CHEERING 3_REVERB
#WIGGLE_NORMAL
Lucía: QUE PASE EL PRIMER NOVIOOOOO!!! //Reaccion del publico Aplausos
#NO_WIGGLE
#HIDE_LUCÍA
#SHOW_HÉCTOR_LEFT
#SHOW_LUCÍA_CENTER
Narrador: Llega Héctor y abraza a Rocío antes de tomar asiento.
Héctor: Buenas con todos, un gusto haber sido invitado.

*[Uy, tú de ley eres el del Gasto porque con esas fachas... olvídate papito. De Gusto no tienes nada.]
    #SFX_RATING UP MEDIUM_ECHO
    #SFX_SITCOMLAUGHTER
    ~ episode_rating += 20 // reaccion del publico Risas
    Rocío: Lucía, contrólate por favor!
    Héctor: Ehh disculpa, cómo dices?
    Lucía: Olvídalo, pronto verás a lo que me refiero.
    
*[Bienvenido Héctor, cuéntanos cómo conociste a Rocío.]
    #SFX_RATING UP SMALL_ECHO
    ~ episode_rating += 5 // reaccion del publico Ternura
    #SFX_AUDIENCE AWWW 1_REVERB
    Héctor: Nos conocimos en nuestros ensayos de baile urbano, desde que la vi quedé perdidamente enamorado de ella.

- Lucía: Y tienes alguna idea de por qué estás aquí?
Héctor: Pues la verdad no, Rocío dijo que tenía una sorpresa para mí y que podía salir en televisión. Y heme aquí.
Lucía: Pues enorme sorpresa la que te vas a llevar.
#WIGGLE_NORMAL
Lucía: QUE PASE EL SEGUNDO NOVIOOOOO!!!
#NO_WIGGLE
#HIDE_LUCÍA
#SHOW_ISAAC_CENTER
Héctor: Espera, cómo que segundo novio???!!!!
// reaccion del publico Emoción y Aplausos
Narrador: Llega Isaac y se acerca para besar a Rocío.
Héctor: Hijo de la gran...
Narrador: Héctor se abalanza sobre él y comienzan a caerse a golpes.
//reacion del publico asombro
Lucía: Ave maría purísima, se armó la grande.
Rocío: Se van a lastimar, alguien haga algo!!!

* [Dejar que se saquen la madre]
#SFX_RATING UP LARGE_ECHO
#SFX_PELEA CAMPANA
~ episode_rating += 20
// info a un lado que diga (i: decidiste no interrumpir la pelea)

* [Llamar a seguridad]
// info a un lado que diga (i: decidiste interrumpir la pelea)
#SFX_RATING DOWN LARGE_ECHO
~ episode_rating -= 20

- Lucía: Ya mucha tontera, se me calman los dos. O resuelven esto como adultos o los expulso de mi set!!!
Narrador: Los dos vuelven a sus asientos y todos hacen silencio en la sala.

Lucía: Está bien, podemos proseguir... Rocío, les debes una explicación a estos muchachos.
Narrador: Hector e Isaac dirigen su mirada a Rocío.

Rocío: Jeje hola chicos, pues verán... los dos son maravillosos y me siento tan afortunada de tenerlos!!!
Rocío: Porque uno es tan guapo que pone celosas a todas mis amigas de lo bueno que está y el otro me cumple todos mis caprichos y me consiente.
Rocío: No veo por qué no podemos continuar con esto tan especial que tenemos. Es como dice el dicho. "Lo que no es en tu año, no te hace daño" (guiño, guiño)

* [Sé que eres una buena chica, solo necesitas amor, comprensión y ternura.]
     #SFX_RATING DOWN MEDIUM_ECHO
    ~ episode_rating -= 10 // reaccion del publico Enojo
    // info a un lado que diga (i: decidiste apoyar a Rocío)
    Rocío: Gracias Lucía, eres la mejor!
    
* [Dios mío, pero qué conchuda que eres!]
      #SFX_RATING UP MEDIUM_ECHO
    ~ episode_rating += 10 // reaccion del publico Risas public_reaction = "enojo"
    // info a un lado que diga (i: decidiste regañar a Rocío)
    Rocío: Lucíaaa qué te pasa, no me hagas quedar como la mala.

- Lucía: Isaac, te concedo la palabra ya que no tuviste la oportunidad de presentarte. Pero te lo advierto, nada de insultos ni provocaciones, ese es mi trabajo!

Isaac: Te lo agradezco Lucía, pues yo soy el verdadero novio de Rocío y estoy dispuesto a todo para estar con ella. Porque la amo de verdad.
    //public_reaction = "ternura"

* [Eso es muy simp beta cuck de tu parte, pero lo respeto.]
    #SFX_RATING DOWN SMALL_ECHO
    ~ episode_rating -= 5
    //public_reaction = "ternura"
    
* [Estás conciente de que te está poniendo los cachos, verdad?]
    #SFX_RATING UP SMALL_ECHO
    ~ episode_rating += 5
    //public_reaction = "risas"
    Rocío: Lucíaaaaaa :(

- Isaac: Yo la perdono, es mujer y estar con varios a la vez es parte de la naturaleza femenina. Es como dice Armando Guerra en sus videos. 
Isaac: Se le estimuló la hipergamia. Claramente está confundida y necesita que yo tome las decisiones por ella.
    //public_reaction = "indignación"
    
Narrador: Lámpara este man oe. Lucía pone cara de enojo...
#SFX_APPLAUSE1
Lucía: Ok, primero que nada la palabra "Hipergamia" queda prohibida en mi set.
Lucía: Segundo, acaso estás diciendo que las mujeres somos todas unas infieles y unas incapaces? Eso no te lo voy a permitir!

Héctor: Lucía, me permites romperle la nariz a este ridículo?
Isaac: Ja, solo estás celoso porque Rocío prefiere estar con un hombre proveedor como yo. 
Lucía: “Hombre proveedor?”... Así que tú eres el del Gasto!!!
#SFX_MONEY1
Isaac: Así es y a mucha honra!
Lucía: Rocío por qué estás tan callada, vas a dejar que este sinvergüenza se exprese así de ti y de todas nosotras?
Rocío: ... Es que... Lucía... nunca nadie se había preocupado tanto por mí!
Lucía: No seas ridícula, por favor.

Carmen (vía intercom): Eh... Lucía, producción me comenta que tienen un Testigo Sorpresa.
Narrador: Todos en la sala quedan desconcertados y confundidos.
Lucía: Maravillosas noticias Carmencita, hazlo pasar. 
Isaac: Espera, no puedes meter a cualquier random aquí. Este problema es de los tres nada más.
Lucía: Este es mi show, papito, y aquí mando YO!!! 
#MUSIC_ACTION
#WIGGLE_NORMAL
Lucía: QUE PASE EL TESTIGOOOO!!!!
#NO_WIGGLE
Xavier: Hola mucho gusto, soy Xavier el novio de Rocío.
//Isaac pone cara de alivio, secuencia donde lo disimula
Lucía: (viendo con rabia a Rocio).
Lucía: Tienes...otro...novio...???
Narrador: Todos voltean a ver a Rocío con indignación.
// reaccion del publico: Indignacion
Rocío: Lo conoci ayer...jeje... es que... tiene carro y ya pues... (risas nerviosas)

* [Alcahuetear a Rocío]
    #SFX_RATING DOWN LARGE_ECHO
    ~ episode_rating -= 20
    ->alcahueta
    
* [Funar a Rocío]
    #SFX_RATING UP LARGE_ECHO
    ~ episode_rating += 20
    ->funaRocio
    
    
=== funaRocio ===
Lucía: Sabes Rocío, a lo largo de este caso he sido demasiado paciente contigo. Te di el beneficio de la duda pero ya es suficiente.
Narrador: Rocío no puede más y estalla en cólera.
Rocío: Bueno ya, sí lo admito soy una interesada!
Lucía: Ya era hora de que lo reconocieras...
Rocío: Y ahora a dónde me van a mandar, a la Penitenciaria?
Todos al unísono: NO!
Rocío: Entonces a dónde me van a mandar, a DespiérTC para que Caterva me putee?
Todos al unísono: TAMPOCO!
Lucía: A ver, a veeeer Rocío, sin lisuras!!!!
Isaac: Rocío, mi vida no es necesario que grites. Eso no es digno de una mujer de alto valor como tú.
Lucía: Héctor, la próxima vez que Isaac diga una ridiculez como esa. Tienes total libertad de meterle un puñete en toda la jeta.
Héctor: (Con una sonrisa discreta) Te lo agradezco Lucía.

-> opinionExterna
    
=== alcahueta ===
Lucía: Estoy de tu parte Rocío, sé que has cometido errores pero aún puedes redimirte.
Rocío: Lucía, yo quiero un hombre bueno, que me cuide, que me proteja...

* [Que te MATENGA]
    #SFX_RATING UP MEDIUM_ECHO
    ~ episode_rating += 10
    // reaccion del publico: Risas
    
* [Que te AME]
    #SFX_RATING UP SMALL_ECHO
    ~ episode_rating += 5
    #SFX_AUDIENCE AWWW 1_REVERB
    // reaccion del publico: Awww

- Lucía: Hmmm debe haber alguna forma de resolver esto de manera racional y madura.

-> opinionExterna

=== opinionExterna ===
Lucía: Creo que es momento de pedir una opinión externa.
* [Que pasen los expertos!]
    Lolita (vía intercom): Lo lamento Lucía, me temo que no hemos aprobado el presupuesto para eso todavía.
    Lucía: Y hasta ahora me lo dicen?
    Carmen (vía intercom): Jeje, se me pasó por alto. Una disculpa jefecita.
    Lolita (vía intercom): Mejor intenta pidiéndole al público su opinión, de seguro tienen algo interesante que decir.
    ->opinionExterna
    
* [Preguntémosle al público!]
    #SFX_RATING UP MEDIUM_ECHO
    ~ episode_rating += 10
    Narrador: Le dan el micrófono a un miembro random del público y este sube al escenario.
    Lucía: Hola qué tal, gracias por acompañarnos hoy en el show. Cuál es tu nombre?
    Poncharelo: Un gusto mi estimada Lucía, me llamo Poncharelo.
    Lucía: Poncharelo dinos, qué piensas de todo esto embrollo?
    Poncharelo: Escúcheme Señorita C-Dura, puedo llamarla así? 
    Lucía: Pero claro!
    Poncharelo: Pues si me permite decirlo...
    #SFX_DRAMA1
    Poncharelo: Estoy verdaderamente arrecho!
    Lucía: Hey, cuidado con ese vocabulario por favor.
    Poncharelo: Discúlpeme, Señorita C-Dura. Pero yo pienso que estos son unos...
    #SFX_DRAMATIC VINE
    Poncharelo: Pobres.
    #SFX_DRAMATIC VINE
    Poncharelo: Tristes.
    #SFX_DRAMATIC VINE
    Poncharelo: Sinvergüenzas!
    #SFX_DRAMA2
    Poncharelo: *Señalando a Rocío* Usted mijita!
    Rocío: Yo qué, sapo?
    #SFX_AIRHORN
    Poncharelo: Se me hace que tú lo que quieres es un cajero automático, una cara bonita, y un chofer, no una pareja.
    #SFX_AUDIENCE CHEERING 3_REVERB
    Narrador: La audiencia aplaude.
    #SFX_DRAMA3
    Poncharelo: *Señalando a Isaac, Héctor y Xavier* Y ustedes pelados. 
    // cada no de ellos con expresiones distintas, isaac smug enojado, hector asustado y xavier confundido
    Isaac: ...
    Héctor: ...
    Xavier: ...
    #SFX_DRAMA2
    Poncharelo: Despierten de ese dulce sueño porque son tremendos cachudos!!!
    #SFX_AUDIENCE CHEERING 1_REVERB
    Poncharelo: Dejen de pelearse por esta trepadora.
    Narrador: El público enloquece.
    Lucía: Ah caray, se los está papeando...
    Lucía: Te agradezco Poncharelo por tu honesta reacción a nuestro caso de hoy. 

    Poncharelo: No hay de qué, Señorita Lucía.
    Narrador: Poncharelo vuelve a su asiento.
    Xavier: ...Bueno, creo que yo me retiro.
    Lucía: Espera, te vas tan rápido?
    #SFX_MONEY1
    Xavier: Es que me cobran 5 latas la hora en el parqueadero.
    Lucía: ... 
    Isaac: Otro chiro tenía que ser.
    Lucía: Hmmm
    Hector:

    Lucía: Bueno mis queridos televidentes, espero estén disfrutando del programa.
    #MUSIC_STOP
    #SFX_COMMERCIAL BREAK
    #WIGGLE_NORMAL
    Lucía: Volveremos después de estos breves cortes comerciales!
    #NO_WIGGLE
    #MUSIC_CHILL
    Narrador: Los panelistas se retiran a camerinos. Héctor cabizbajo, Isaac con una expresión de alivio y Rocío con semblante reflexivo.
   
    Narrador: Carmen y Lolita se acercan al escenario.
    #WIGGLE_NORMAL
    Carmen: Estás haciéndolo de maravilla, Lucy!!!
    #NO_WIGGLE
    Lolita: El show ha superado mis expectativas, estupendo trabajo.
    Lucía: Gracias por su apoyo chicas, estoy divi
    Lucía: No soy la unica que ha estado escuchando cosas. Verdad?
    Lolita: Qué clase de cosas?
    
    #MUSIC_DEFAULT SHOW THEME


//todos se cargan a Rocio hasta que aparece un ultimo testigo. Alguien que vi el show en este momento y esta afuera del estudio tratando de entrar.
- 


//Cortes comerciales y se habla 


#MUSIC_MYSTERY
Lolita (vía intercom): Lucía, tenemos una situación.
Lucía: Qué sucede?
Lolita (vía intercom): Es increíble, recibimos una llamada de alguien que estaba viendo el programa y nos pide urgen. 
Lucía: Pues, 
Carmen (vía intercom): Está en el backstage ahora mismo...
Lucía: Ah, está bien. Que pase.


-> DONE

-> END