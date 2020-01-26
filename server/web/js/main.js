let margins = {
  top: 30,
  right: 10,
  bottom: 10,
  left: 60
};

let width = 400 - margins.left - margins.right;
let height = 1000 - margins.top - margins.bottom;

let xScale = d3.scaleLinear().domain([0, 1]).range([0, width]);
let yScale = d3.scaleTime().range([0, height]);

let xAxis = d3.axisTop(xScale);
let yAxis = d3.axisLeft(yScale);
let yAxisGrid = d3.axisLeft(yScale).tickSize(-width).tickFormat("");

let graph = d3.select("#emotion-graph")
  .append("g")
  .attr("transform", "translate(" + margins.left + "," + margins.top + ")");

let line = d3.line()
  .x(function(d) { return xScale(d.stress); })
  .y(function(d) { return yScale(d.date); });
  //.curve(d3.curveBasisOpen);
let area = d3.area()
  .x0(xScale(0))
  .x1(function(d) { return xScale(d.stress); })
  .y(function(d) { return yScale(d.date); });

d3.json("/data/hapiness.json").then(function(data) {
  for(var i = 0; i< data.length; i++) {
    data[i].date = d3.timeParse("%Y-%m-%dT%H:%M:%S")(data[i].date)
  }

  yScale.domain(d3.extent(data, function(element) { return element.date; }));

  // graph.append("path")
  //   .attr("d", line(data))
  //   .attr("clip-path", "url(#clip)")
  //   .style("stroke", "blue")
  //   .style("fill", "none");
  
  graph.append("path")
    .attr("d", area(data))
    .style("stroke", "none")
    .style("fill", "url(#Gradient1)")
    .attr("clip-path", "url(#clip)");
  
  graph.append("g")
    .attr("class", "x axis")
    .attr("transform", "translate(0," + 0 + ")")
    .call(xAxis);
  
  // y axis
  graph.append("g")
    .attr("class", "y axis")
    .call(yAxis);
  // horizontal tick lines
  graph.append("g")
    .attr("class", "y axis")
    .style("stroke-dasharray",("3,3"))
    .style("color", "")
    .call(yAxisGrid);


  console.log(data);
});
