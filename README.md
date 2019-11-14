# Enterprise Integration Patterns :office::chains::scroll:

The main objective of this repository is to demonstrate the use of :bridge_at_night: integration platforms and middleware. Insights about the best practices:medal_military:for integrating enterprise applications were gained from the book:closed_book:[Enterprise Integration Patterns](https://www.enterpriseintegrationpatterns.com) _by Gregor Hohpe_.

</br>

---
## Getting Started :clapper:
NB:bangbang: **Before** running our 2 projects, make sure you have _RabbitMQ_ running :rabbit:.  

If you have _Docker_ within reach :whale: you can simply run the following command to start it:
<p align="center"><code>docker run --rm --name rabbitMQ -p 15672:15672 -p 5672:5672 rabbitmq:management</code></p>  

> Optional: To make sure _RabbitMQ_ is running, navigate to `localhost:15672`in your browser


</br>

---
## Business Scenario :briefcase:
Writtend description of the depicked scenario can be found in a [pdf format](https://github.com/datsoftlyngby/soft2019fall-lsd-teaching-material/blob/master/week36/case_car_article.pdf).

<p align="center">
<img src="BPMN_CarRental.png">
<em>BPMN</em>
</p>

</br>

---
## Application architecture :building_construction:

</br>

---
## Software Implementation :keyboard:

Our solution illustrates the implementation of at least :five: enterprise integration patterns:
- `Messaging Gateway` - as a class to wrap messaging-specific method calls
- `Correlation Identifier` - assigns the request a request ID that will be used to processes the reply ( by correlation Id - we know which request the reply is for)
- `Point-To-Point` - RPC
- `Publish-Subscribe Channel` - when sending notifications
- `Message-Translator` - translate one data format into another
- `Command Message`
- Others

The data processed by the application exists in two different formats - JSON and XML.

</br>

---
> #### Assignment made by:   
`David Alves 👨🏻‍💻 ` :octocat: [Github](https://github.com/davi7725) <br />
`Elitsa Marinovska 👩🏻‍💻 ` :octocat: [Github](https://github.com/elit0451) <br />
> Attending "System Integration" course of Software Development bachelor's degree
