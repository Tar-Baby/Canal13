VAR show_name = ""
VAR episode_rating = 0  //luego agregar public_reaction
VAR public_reaction = "neutral"

//#FADEALL
// Inicio del programa principal
Narrador: Al aire en 3...2...1...
#SHOW_LUCÍA_LEFT
//#EXPRESSION_LUCÍA_SALUDO este es su expresion default en este caso
Lucía: Hola a todos, bienvenidos al gran estreno de "{show_name}" el día de hoy tenemos un programa espectacular! 

#EXPRESSION_LUCÍA_FRASE
Lucía: Y quisiera comenzar con una frase, se trata de un antiguo proverbio montubio que dice así:
Lucía: "El que la hace, se olvida. El que la recibe, nunca"
#EXPRESSION_LUCÍA_QUE PASEEE
Lucía: Que pase nuestra invitada especial, démosle un aplauso a Rocíooo.

Narrador: (El público enloquece)
#SHOW_ROCÍO_RIGHT
Rocío: Hola Lucía es un honor estar ante ustedes y ante las cámaras. Gracias por recibirme en tu programa.

#EXPRESSION_LUCÍA_NORMAL
Lucía: El placer es todo mío, bueno cuéntanos qué te trae aquí hoy.

#EXPRESSION_ROCÍO_NORMAL
Rocío: Verás Lucía, estoy enfrentando la decisión más difícil en la vida de toda mujer.

// OPCIONES DEL USUARIO (en rosado en el diagrama)
* [Vaya, si lo pones así... dime más!!!]
    ~ episode_rating += 5
    // Esta es la respuesta del usuario/jugador
    -> continue_story //usar estos Diverts para sacar el Final Bueno y Final Malo

* [Ay, tampoco exageres pues mijita.]
    ~ episode_rating += 10
    // Esta es la respuesta del usuario/jugador
    -> continue_story

= continue_story
// Aquí continúa independientemente de la opción elegida

#EXPRESSION_ROCÍO_ENAMORADA1
#EXPRESSION_LUCÍA_CONMOVIDASOFT
Rocío: Estoy enamorada...
// reaccion del publico Ternura
#EXPRESSION_ROCÍO_ENAMORADA2
#EXPRESSION_LUCÍA_SORPRENDIDA1
Rocío: De dos a la vez... 
// reaccion del publico Asombro

#EXPRESSION_LUCÍA_CONFUNDIDA1
Lucía: Espérate, pérate. Cómo es eso?
#EXPRESSION_ROCÍO_BANDIDA1
Rocío: Tal y como escuchaste Lucía. Llevo saliendo ya un buen tiempo con dos chicos que conocí en la academia de baile en la que estudio.
#EXPRESSION_ROCÍO_FELIZINDECISA
#EXPRESSION_LUCÍA_DUDOSA1
Rocío:  Y la verdad es que ambos me hacen muy feliz. Los amo a los dos!
//#HIDE_LUCÍA para peobar que funcione el FadeOut
* [Tranquila reina nosotros te ayudaremos a resolver este triángulo amoroso.]
    ~ episode_rating += 5
    Rocío: Gracias Lucía, sabía que podía contar con tu apoyo!
    
* [Los amas a los dos, Rocío? Hmm... no lo sé, algo me huele raro aquí.]
    ~ episode_rating += 10
    Rocío: Ay no seas así, déjame explicarte antes de que saltes a conclusiones.
    
- Lucía: Bueno, bueno cómo es la vaina?
Rocío: Mira, la razón por la que tengo dos novios es sencilla. Tengo a uno para el Gusto y otro para el Gasto. 
    //~ episode_rating += 10 // reaccion del publico Asombro public_reaction = "asombro"

* [Tal y cómo lo sospeché. Eres una bandida!]
    ~ episode_rating += 10   // reaccion del publico Risas
    
* [No sé si termino de comprender, pero tengo miedo de preguntar.]
    ~ episode_rating -= 5
    
- Rocío: (se sonroja) A ver, pero qué culpa tengo yo de que el guapo sea chiro y el del billete sea bagre?
Rocío: Por eso quiero que me ayudes a decidirme por uno!!! 
Lucía: Ya ya entendí... La plena que esto se pone cada vez mejor, que pase el primer noviooooo Héctor!! //Reaccion del publico Aplausos

Narrador: (Llega Héctor y abraza a Rocío antes de tomar asiento)
Héctor: Buenas con todos, un gusto haber sido invitado.

*[Uy, tú de ley eres el del Gasto porque con esas fachas... olvídate papito. De Gusto no tienes nada.]
    ~ episode_rating += 10 // reaccion del publico Risas
    Rocío: Lucía, contrólate por favor!
    Héctor: Ehh disculpa, cómo dices?
    Lucía: Olvídalo, pronto verás a lo que me refiero.
    
*[Bienvenido Héctor, cuéntanos cómo conociste a Rocío.]
Héctor: Nos conocimos en nuestros ensayos de baile urbano, desde que la vi quedé perdidamente enamorado de ella.
    ~ episode_rating += 5 // reaccion del publico Ternura

- Lucía: Y tienes alguna idea de por qué estás aquí?
Héctor: Pues la verdad no, Rocío dijo que tenía una sorpresa para mí y que podía salir en televisión. Y heme aquí.
Lucía: Pues enorme sorpresa la que te vas a llevar, que pase el segundo noviooooo Isaac!!!
Héctor: Espera, cómo que segundo novio???!!!!
// reaccion del publico Emoción y Aplausos
Narrador: (Llega Isaac y se acerca para besar a Rocío)
Héctor: Hijo de la gran...
Narrador: (Héctor se abalanza sobre él y comienzan a caerse a golpes)
//reacion del publico asombro
Lucía: Ave maría purísima, se armó la grande.
Rocío: Se van a lastimar, alguien haga algo!!!

* [Dejar que se saquen la madre]
~ episode_rating += 20
// info a un lado que diga (i: decidiste no interrumpir la pelea)

* [Llamar a seguridad]
// info a un lado que diga (i: decidiste interrumpir la pelea)
~ episode_rating -= 10

- Lucía: Ya mucha tontera, se me calman los dos. O resuelven esto como adultos o los expulso de mi set!!!
Narrador: (Los dos vuelven a sus asientos y todos hacen silencio en la sala)

Lucía: Está bien, podemos proseguir... Rocío, les debes una explicación a estos muchachos.
Narrador: Hector e Isaac dirigen su mirada a Rocío.

Rocío: Jeje hola chicos, pues verán... los dos son maravillosos y me siento tan afortunada de tenerlos!!!
Rocío: Porque uno es tan guapo que pone celosas a todas mis amigas de lo bueno que está y el otro me cumple todos mis caprichos y me consiente.
Rocío: No veo por qué no podemos continuar con esto tan especial que tenemos. Es como dice el dicho. "Lo que no es en tu año, no te hace daño" (guiño, guiño)

* [Sé que eres una buena chica, solo necesitas amor, comprensión y ternura.]
    ~ episode_rating += 5 // reaccion del publico Enojo
    // info a un lado que diga (i: decidiste apoyar a Rocío)
    Rocío: Gracias Lucía, eres la mejor!
    
* [Dios mío, pero qué conchuda que eres!]
    ~ episode_rating += 10 // reaccion del publico Risas public_reaction = "enojo"
    // info a un lado que diga (i: decidiste regañar a Rocío)
    Rocío: Lucíaaa, vine al programa porque se supone que debes ayudarme!!!! No hacerme quedar como la mala :(

- Lucía: Isaac, te concedo la palabra ya que no tuviste la oportunidad de presentarte. Pero te lo advierto, nada de insultos ni provocaciones, ese es mi trabajo!

Isaac: Te lo agradezco Lucía, pues yo soy el verdadero novio de Rocío y estoy dispuesto a todo para estar con ella. Porque la amo de verdad.
    //public_reaction = "ternura"

* [Eso es muy simp beta cuck de tu parte, pero lo respeto.]
    ~ episode_rating -= 5
    //public_reaction = "ternura"
    
* [Estás conciente de que te está poniendo los cachos, verdad?]
    ~ episode_rating += 5
    //public_reaction = "risas"
    Rocío: Lucíaaaaaa :(

- Isaac: Yo la perdono, es mujer y estar con varios a la vez es parte de la naturaleza femenina. Es como dice Armando Guerra en sus videos. 
Isaac: Se le estimuló la hipergamia. Claramente está confundida y necesita que yo tome las decisiones por ella.
    //public_reaction = "indignación"
    
Lucía pone cara de enojo...

Lucía: Ok, primero que nada la palabra "Hipergamia" queda prohibida en mi set.
Lucía: Segundo, acaso estás diciendo que las mujeres somos todas unas infieles y unas incapaces? Eso no te lo voy a permitir!
    ~ episode_rating += 10
    //public_reaction = "aplausos"

Héctor: Lucía, me permites romperle la nariz a este ridículo?
Isaac: Ja, solo estás celoso porque Rocío prefiere estar con un hombre proveedor como yo. 
Lucía: “Hombre proveedor?”... Así que tú eres el del Gasto!!!
Isaac: Así es y a mucha honra!
    //public_reaction = "sorpresa"
Lucía: Rocío por qué estás tan callada, vas a dejar que este sinvergüenza se exprese así de ti y de todas nosotras?
Rocío: ... es que... Lucía... nunca nadie se había preocupado tanto por mí!
Lucía: Ay bebé. Estás viendo y no ves...

Carmen (via intercom): Eh... Lucía, producción me comenta que tienen un Testigo Sorpresa.
Lucía: Entendido Carmencita, hazlo pasar.
Rocío: Espera, eso es imposible. Debe ser un malentendido, él no puede estar aquí! 
Lucía: Este es mi show y aquí mando yo!!! Que pase el Testigoooooo!!!!

-> END