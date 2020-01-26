let margins = {
  top: 0,
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

let graph_defs = d3.select("#emotion-graph").select("defs");
let graph = d3.select("#emotion-graph")
  .append("g")
  .attr("transform", "translate(" + margins.left + "," + margins.top + ")");

let risky_points = graph.selectAll(".risky-pts");

let top_bar = d3.select("#emotion-graph-top-bar")
  .append("g")
  .attr("transform", "translate(" + margins.left + "," + 0 + ")");

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

  yScale.domain(d3.extent(data, function(element) { return element.date; })).invert();

  // graph.append("path")
  //   .attr("d", line(data))
  //   .attr("clip-path", "url(#clip)")
  //   .style("stroke", "blue")
  //   .style("fill", "none");
  
  graph.append("path")
    .attr("d", area(data))
    .style("stroke", "none")
    .style("fill", "#001330")
    .attr("clip-path", "url(#clip)");
  
  top_bar.append("g")
    .attr("class", "x axis")
    .style("color", "#ffffffff")
    .style("stroke", "#ffffffff")
    .attr("transform", "translate(0," + 19 + ")")
    .call(xAxis);
  
  
  // y axis
  graph.append("g")
    .attr("class", "y axis")
    .call(yAxis);
  // horizontal tick lines
  graph.append("g")
    .attr("class", "y axis")
    .style("stroke-dasharray",("3,3"))
    .style("color", "#ffffffff")
    .call(yAxisGrid);

  let marker_radius = 6;
  let marker_color = "darkred";
  graph_defs.append("clipPath")
    .attr("id", "notification-clip-path")
    .append("circle")
    .attr("cx", 0)
    .attr("cy", 0)
    .attr("r", marker_radius - 1);

  let risky_points_binding = risky_points.data(data.filter(function(d) { return d.stress >= 0.8; }));
  let marker = risky_points_binding.enter()
    .append("g")
    .on("mouseover", handleMouseOver)
    .on("mouseout", handleMouseOut)
    .attr("transform", function(d) { return "translate(" + xScale(d.stress) + "," + yScale(d.date) + ")"; })
    .append("g");
  marker.append("circle")
    .attr("cx", 0)
    .attr("cy", 0)
    .attr("r", marker_radius)
    .style("fill", marker_color)
  //   .on("mouseover", handleMouseOver)
  //   .on("mouseout", handleMouseOut);
  marker.append("image")
    .attr("x", -marker_radius)
    .attr("y", -marker_radius)
    .attr("width", 2*marker_radius)
    .attr("height", 2*marker_radius)
    .attr("xlink:href", "/logo.png")
    .attr("clip-path", "url(#notification-clip-path)");
    
  function handleMouseOver(d, i) {
    let g = d3.select(this)
      .raise()
      .select("g");
    
    g.transition()
      .duration(300)
      .attr("transform", "scale(" + 4 + ")")
      .attr("z", 2);
    g.append("rect")
      .lower()
      .attr("width", 11 * marker_radius)
      .attr("height", 2*marker_radius)
      .attr("x", -11 * marker_radius)
      .attr("y", -marker_radius)
      .style("fill", marker_color);
    graph.append("text")
      .attr("id", "t" + i) // Create an id for text so we can select it later for removing on mouseout
      .attr("x", function() { return xScale(d.stress) - 30; })
      .attr("y", function() { return yScale(d.date) - 15; })
      .text(function() {
        return [d.date, d.stress];
      }); 
  }

  function handleMouseOut(d, i) {
    let g = d3.select(this)
      .select("g");
    g.transition()
      .duration(300)
      .attr("transform", "")
      .attr("z", 0);
    g.select("rect")
      .remove();
    d3.select("#t" + i).remove();
  }
});
