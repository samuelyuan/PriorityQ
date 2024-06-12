# PriorityQ

<div style="display:inline-block;">
<img src="https://raw.githubusercontent.com/samuelyuan/PriorityQ/master/screenshots/homepage.png" alt="homepage" width="400" height="250" />
</div>

<div style="display:inline-block;">
<img src="https://raw.githubusercontent.com/samuelyuan/PriorityQ/master/screenshots/about1.png" alt="about1" width="400" height="250" />
<img src="https://raw.githubusercontent.com/samuelyuan/PriorityQ/master/screenshots/about2.png" alt="about2" width="400" height="250" />
</div>

PriorityQ is designed to make waiting for restaurants as efficient as possible.

In the frontend, the site tells you if a restaurant is filled (tables available) and how long you have to wait (groups waiting). You can check this before you visit the restaurant. No need to go there only to find out it's full. 

In the backend, the restaurants will update the information in real time. If there's available tables, assign customer to that table. If there's no open tables, the customer is added to a priority queue and as soon as a table is available for a customer, the customer is assigned to that table.